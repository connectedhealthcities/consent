using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.IdentifierDisplay;
using CHC.Consent.Api.Infrastructure.Identity;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Consent.Identifiers;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.DependencyInjection;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Identity;
using IdentityModel;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api
{
    public class Startup
    {
        private static readonly string MigrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AddConsentSystemTypeRegistrations(services);
   
            services
                .AddMvc(
                    options =>
                    {
                        options.ReturnHttpNotAcceptable = true;
                        options.RespectBrowserAcceptHeader = true;
                    })
                .AddFeatureFolders()
                .AddJsonOptions(
                    config =>
                    {
                        SerializerSettings(config.SerializerSettings);
                    })
                .AddRazorPagesOptions(o => o.Conventions.AuthorizeFolder("/"));


            var identityServerConfiguration =
                Configuration.GetSection("IdentityServer").Get<IdentityServerConfiguration>();
            
            services
                .AddAuthentication()
                .AddCookie("Monster")
                .AddOpenIdConnect(
                    options =>
                    {
                        options.SignInScheme = "Monster";
                        options.Authority = identityServerConfiguration.Authority;
                        options.ClientId = "UI";
                        if (Environment.IsDevelopment())
                            options.RequireHttpsMetadata = false;
                        options.SaveTokens = true;
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.GetClaimsFromUserInfoEndpoint = true;                        
                    });
            services.AddTransient<IUserProvider, HttpContextUserProvider>();

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGenOptionsProvider>();
            services.AddSwaggerGen(_ => {});
            
            AddDataServices(services);
        }


        private void AddDataServices(IServiceCollection services)
        {
            services.AddScoped<IIdentityRepository, IdentityRepository>();
            services.AddScoped<IConsentRepository, ConsentRepository>();
            services.AddScoped<ISubjectIdentifierRepository, SubjectIdentifierRepository>();

            services.AddDbContext<ConsentContext>(
                ConfigureDatabaseOptions);
            services.AddScoped<IStoreProvider>(
                provider => new ContextStoreProvider(provider.GetService<ConsentContext>()));
            services.AddScoped(typeof(IStore<>), typeof(Store<>));
        }

        private void AddConsentSystemTypeRegistrations(IServiceCollection services)
        {
            services.AddPersonIdentifiers(
                registry =>
                {
                    registry.Add<NhsNumberIdentifier>(o => o.WithMarshaller<NhsNumberIdentifierMarshaller>());
                    registry.Add<BradfordHospitalNumberIdentifier>(
                        o => o.WithMarshaller<BradfordHospitalNumberIdentifierMarshaller>());
                    registry.Add<MedwaySexIdentifier>(o => o.WithMarshaller<SexIdentifierMarshaller>());
                    registry.Add<DateOfBirthIdentifier>(o => o.WithXmlMarshaller(valueType: "dateOfBirth"));
                    registry.Add<MedwayNameIdentifier>(o => o.WithXmlMarshaller(valueType: "BIB4All.MedwayName"));
                    registry.Add<MedwayAddressIdentifier>(o => o.WithXmlMarshaller(valueType: "BIB4All.MedwayAddress"));
                    registry.Add<MedwayContactNumberIdentifier>(
                        o => o.WithXmlMarshaller(valueType: "BIB4All.MedwayContactNumber"));
                    registry.Add<MedwayBirthOrder>(o => o.WithXmlMarshaller(valueType: "BIB4All.MedwayBirthOrder"));
                }
            );

            services.AddTransient<IPersonIdentifierDisplayHandlerProvider, PersonIdentifierHandlerProvider>();

            var consentIdentifierRegistry = new TypeRegistry<CaseIdentifier, CaseIdentifierAttribute>();
            consentIdentifierRegistry.Add<PregnancyNumberIdentifier>();
            services.AddSingleton(consentIdentifierRegistry);
            services.AddSingleton<ITypeRegistry<CaseIdentifier>>(consentIdentifierRegistry);

            var evidenceRegistry = new EvidenceRegistry();
            evidenceRegistry.Add<MedwayEvidence>();
            evidenceRegistry.Add<ImportFileEvidence>();
            services.AddSingleton(evidenceRegistry);
            services.AddSingleton<ITypeRegistry<Evidence>>(evidenceRegistry);

            services.AddSingleton<ConsentTypeRegistry>();


            //These setup the relevant body binders
            services
                .AddTransient<
                    IPostConfigureOptions<MvcOptions>,
                    IdentityModelBinderProviderConfiguration<ITypeRegistry<IPersonIdentifier>, PersonSpecification>>();
            services
                .AddTransient<
                    IPostConfigureOptions<MvcOptions>, 
                    IdentityModelBinderProviderConfiguration<ITypeRegistry<IPersonIdentifier>, MatchSpecification>>();
            services
                .AddTransient<
                    IPostConfigureOptions<MvcOptions>, 
                    IdentityModelBinderProviderConfiguration<ConsentTypeRegistry, ConsentSpecification>>();


            services.Configure<IdentifierDisplayOptions>(
                displayOptions => Configuration.Bind("IdentifierDisplay", displayOptions));

        }

        protected virtual void ConfigureDatabaseOptions(IServiceProvider provider, DbContextOptionsBuilder options)
        {
            options.UseSqlServer(Configuration.GetConnectionString("Consent"));
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            if (Configuration.GetIdentityServer().EnableInteralServer)
            {
                app.UseIdentityServer();
            }

            app.UseStaticFiles();
            app.UseMvc(r => r.MapRoute("default", "{controller=home}/{action=index}"));
            app.UseSwagger();
            app.UseSwaggerUI(ui =>
            {
                ui.SwaggerEndpoint("/swagger/v1/swagger.json", "Api");
            });
        }

        public static JsonSerializerSettings SerializerSettings(JsonSerializerSettings settings)
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Formatting = Formatting.Indented;
            return settings;
        }
    }
}
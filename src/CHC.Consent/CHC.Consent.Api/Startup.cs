using System;
using System.Collections.Generic;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Consent.Identifiers;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api
{
    public class Startup
    {
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
                /*.AddJsonOptions(
                    config => throw new NotImplementedException("Work In Progress"))*/
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
            services.AddSwaggerGen();
            AddDataServices(services);
        }


        private void AddDataServices(IServiceCollection services)
        {
            services.AddScoped<IIdentityRepository, IdentityRepository>();
            services.AddScoped<IDictionary<string, IIdentifierMarshaller>>(
                delegate(IServiceProvider s)
                {
                    s.GetService<ILoggerProvider>().CreateLogger("Activation")
                        .LogDebug("Creating Marshallers Dictionary");
                    var marshallers = new Dictionary<string, IIdentifierMarshaller>();
                    s.GetService<IdentifierDefinitionRegistryProvider>()
                        .GetRegistry()
                        .Accept(new IdentifierMarshallerCreator(marshallers));
                    return marshallers;
                });
            services.AddScoped<IConsentRepository, ConsentRepository>();
            services.AddScoped<ISubjectIdentifierRepository, SubjectIdentifierRepository>();

            services.AddDbContext<ConsentContext>(ConfigureDatabaseOptions);
            services.AddScoped<IStoreProvider, ContextStoreProvider>();
            
            services.AddScoped(typeof(IStore<>), typeof(Store<>));
        }

        private void AddConsentSystemTypeRegistrations(IServiceCollection services)
        {
            services.AddSingleton<IdentifierDefinitionRegistryProvider>(new IdentifierDefinitionRegistryProvider());
            services.AddTransient(c =>
            {
                c.GetService<ILoggerProvider>().CreateLogger("Activation")
                    .LogDebug("Creating IdentifierDefinitionRegistryProvider");
                return c.GetService<IdentifierDefinitionRegistryProvider>().GetRegistry();
            });
            
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


            //These setup the relevant body binders
            /*services
                .AddTransient<
                    IPostConfigureOptions<MvcOptions>,
                    IdentityModelBinderProviderConfiguration<ITypeRegistry<IPersonIdentifier>, PersonSpecification>>();*/
            /*services
                .AddTransient<
                    IPostConfigureOptions<MvcOptions>, 
                    IdentityModelBinderProviderConfiguration<ITypeRegistry<IPersonIdentifier>, MatchSpecification>>();

            services
                .AddTransient<
                    IPostConfigureOptions<MvcOptions>, 
                    IdentityModelBinderProviderConfiguration<ConsentTypeRegistry, ConsentSpecification>>();


            services.Configure<IdentifierDisplayOptions>(
                displayOptions => Configuration.Bind("IdentifierDisplay", displayOptions));*/

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

            if (Configuration.GetIdentityServer().EnableInternalServer)
            {
                app.UseIdentityServer();
            }

            app.UseStaticFiles();
            app.UseMvc(r => r.MapRoute("default", "{controller=home}/{action=index}"));
            app.UseSwagger();
            app.UseSwaggerUI(ui =>
            {
                ui.SwaggerEndpoint("/swagger/v1/swagger.json", "Api");
                ui.OAuthClientId("ApiExplorer");
                ui.OAuthAppName("API Explorer");
            });
        }
    }
}
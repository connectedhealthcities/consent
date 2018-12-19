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
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Identity;
using IdentityModel;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.Stores.Serialization;
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
using Newtonsoft.Json.Linq;
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
                    config => throw new NotImplementedException("Work In Progress"))
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
            services.AddSingleton<IdentifierDefinitionRegistryProvider>();
            services.AddTransient(c => c.GetService<IdentifierDefinitionRegistryProvider>().GetRegistry());
            
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
            });
        }

        public static JsonSerializerSettings SerializerSettings(
            JsonSerializerSettings settings, IdentifierDefinitionRegistry identifierDefinitionRegistry)
        {
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new PersonIdentifierConverter(identifierDefinitionRegistry));
            settings.SerializationBinder = new IdentifierRegistrySerializationBinder(identifierDefinitionRegistry);
            return settings;
        }
    }

    public class PersonIdentifierConverter : JsonConverter
    {
        public IdentifierDefinitionRegistry Registry { get; }

        public PersonIdentifierConverter(IdentifierDefinitionRegistry registry)
        {
            Registry = registry;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identifier = (PersonIdentifier) value;
            writer.WriteStartObject();
            writer.WritePropertyName("$type", escape: true);

            writer.WriteValue(identifier.Definition.SystemName);


            writer.WritePropertyName("value");
            if (identifier.Definition.Type is CompositeIdentifierType)
            {
                writer.WriteStartObject();
                foreach (var subIdentifier in ((IDictionary<string, PersonIdentifier>) identifier.Value.Value).Values)
                {
                    writer.WritePropertyName(subIdentifier.Definition.SystemName);
                    writer.WriteValue(subIdentifier.Value.Value);
                }

                writer.WriteEndObject();
            }
            else
            {
                writer.WriteValue(identifier.Value.Value);
            }

            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            if (!Registry.TryGetValue((string) jObject["$type"], out var definition)) return null;
            throw new NotImplementedException("This is going to need to be revisited");
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PersonIdentifier) || objectType == typeof(IPersonIdentifier);
        }
    }
}
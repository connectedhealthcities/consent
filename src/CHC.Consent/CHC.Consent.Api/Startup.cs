using System.Reflection;
using System.Runtime.Serialization;
using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Consent.Identifiers;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal.Networking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CHC.Consent.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var personIdentifierRegistry = new PersonIdentifierRegistry();
            personIdentifierRegistry.Add<NhsNumberIdentifier>();
            personIdentifierRegistry.Add<BradfordHospitalNumberIdentifier>();
            personIdentifierRegistry.Add<SexIdentifier>();
            personIdentifierRegistry.Add<DateOfBirthIdentifier>();
            services.AddSingleton(personIdentifierRegistry);
            
            var consentIdentifierRegistry = new ConsentIdentifierRegistry();
            consentIdentifierRegistry.Add<PregnancyNumberIdentifier>();
            services.AddSingleton(consentIdentifierRegistry);
            
            var evidenceRegistry = new EvidenceRegistry();
            evidenceRegistry.Add<MedwayEvidence>();
            services.AddSingleton(evidenceRegistry);

            services.AddSingleton<ConsentTypeRegistry>();

            services.AddTransient<IConfigureOptions<MvcOptions>, IdentityModelBinderProviderConfiguration<PersonIdentifierRegistry, PersonSpecification>>();
            services.AddTransient<IConfigureOptions<MvcOptions>, IdentityModelBinderProviderConfiguration<ConsentTypeRegistry, ConsentSpecification>>();
            
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
                    });
            
            services.AddSwaggerGen(gen =>
            {
                gen.SwaggerDoc("v1", new Info {Title = "Api", Version = "1"});
                gen.DescribeAllEnumsAsStrings();
                gen.SchemaFilter<SwaggerSchemaIdentityTypeProvider<IIdentifier, PersonIdentifierRegistry>>();
                gen.SchemaFilter<SwaggerSchemaIdentityTypeProvider<Identifier, ConsentIdentifierRegistry>>();
                gen.CustomSchemaIds(t => personIdentifierRegistry.GetName(t) ?? consentIdentifierRegistry.GetName(t) ?? t.FriendlyId(fullyQualified:false));
            });
            
            
            services.AddSingleton(typeof(IStore<>), typeof(InMemoryStore<>));
            services.AddSingleton(MakePersonStore());
            services.AddScoped<IdentityRepository>();
            services.AddScoped<IConsentRepository,ConsentRepository>();
        }

        private static IStore<Person> MakePersonStore()
        {
            var peopleStore = new InMemoryStore<Person>();
            peopleStore.OnItemAdded += (store, person) => person.Id = store.Contents.Count;
            return peopleStore;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(ui =>
            {
                ui.SwaggerEndpoint("/swagger/v1/swagger.json", "Api");
            });
        }

        public static JsonSerializerSettings SerializerSettings(JsonSerializerSettings Settings)
        {
            Settings.TypeNameHandling = TypeNameHandling.Auto;
            Settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            Settings.Formatting = Formatting.Indented;
            return Settings;
        }
    }
}
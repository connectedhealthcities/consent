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
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.IdentifierAdapters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            personIdentifierRegistry.Add<NhsNumberIdentifier, NhsNumberIdentifierAdapter>();
            personIdentifierRegistry.Add<BradfordHospitalNumberIdentifier, BradfordHospitalNumberIdentifierAdapter>();
            personIdentifierRegistry.Add<SexIdentifier, SexIdentifierAdapter>();
            personIdentifierRegistry.Add<DateOfBirthIdentifier, DateOfBirthIdentifierAdapter>();
            personIdentifierRegistry.Add<MedwayNameIdentifier, MedwayNameIdentifierAdapter>();
            services.AddSingleton(personIdentifierRegistry);
            services.AddSingleton<IPersonIdentifierListChecker>(personIdentifierRegistry);
            
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
            
            
            services.AddScoped<IIdentityRepository, IdentityRepository>();
            services.AddScoped<IConsentRepository,ConsentRepository>();

            services.AddDbContext<ConsentContext>((provider, options) => { options.UseInMemoryDatabase("CHC.Consent"); });
            services.AddScoped<IStoreProvider>(provider => new ContextStoreProvider(provider.GetService<ConsentContext>()));
            services.AddScoped(typeof(IStore<>), typeof(Store<>));
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
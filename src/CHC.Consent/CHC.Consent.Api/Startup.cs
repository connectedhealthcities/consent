using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Consent.Identifiers;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.DependencyInjection;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
                    registry.Add<MedwayContactNumber>(
                        o => o.WithXmlMarshaller(valueType: "BIB4All.MedwayContactNumber"));
                    registry.Add<MedwayBirthOrder>(o => o.WithXmlMarshaller(valueType: "BIB4All.MedwayBirthOrder"));
                }
            );
            
            
            var consentIdentifierRegistry = new TypeRegistry<ConsentIdentifier,ConsentIdentifierAttribute>();
            consentIdentifierRegistry.Add<PregnancyNumberIdentifier>();
            services.AddSingleton(consentIdentifierRegistry);
            services.AddSingleton<ITypeRegistry<ConsentIdentifier>>(consentIdentifierRegistry);
            
            var evidenceRegistry = new EvidenceRegistry();
            evidenceRegistry.Add<MedwayEvidence>();
            services.AddSingleton(evidenceRegistry);
            services.AddSingleton<ITypeRegistry<Evidence>>(evidenceRegistry);

            services.AddSingleton<ConsentTypeRegistry>();

            services.AddTransient<IPostConfigureOptions<MvcOptions>, IdentityModelBinderProviderConfiguration<ITypeRegistry<IPersonIdentifier>, PersonSpecification>>();
            services.AddTransient<IPostConfigureOptions<MvcOptions>, IdentityModelBinderProviderConfiguration<ConsentTypeRegistry, ConsentSpecification>>();
            
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

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGenOptionsProvider>();
            services.AddSwaggerGen(_ => {});
            
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
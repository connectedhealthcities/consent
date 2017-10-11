using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using CHC.Consent.WebApi.Features.Person;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace CHC.Consent.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore().AddVersionedApiExplorer( o => o.GroupNameFormat = "'v'VVV" );
            services.AddApiVersioning(
                options =>
                {
                    options.DefaultApiVersion = new ApiVersion(0, 1, "dev");
                });
            
            services.AddAuthentication(
                    o =>
                    {
                        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    options =>
                    {
                        Configuration.Bind("Authentication:Jwt", options);
                    });

            services.AddMvc();
            services.AddSwaggerGen(
                c =>
                {
                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                    // add a swagger document for each discovered API version
                    foreach ( var description in provider.ApiVersionDescriptions )
                    {
                        c.SwaggerDoc( description.GroupName,  new Info { Version = description.GroupName });
                    }
                    
                });

            services.AddTransient<IUserAccessor, HttpContextUserAccessor>();

            services.Decorate<IPersonRepository, PersonRepositoryWithSecurity>();
            services.AddTransient<IPersonRepository, PersonRepositoryWithSecurity>()
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvc();
            app.UseSwagger();
        }
    }
    
    public static class ServiceCollectionExtensions
{
    public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, IServiceProvider, TService> decorator)
    {
        var descriptors = services.GetDescriptors<TService>();

        foreach (var descriptor in descriptors)
        {
            services.Replace(descriptor.Decorate(decorator));
        }

        return services;
    }

    public static IServiceCollection Decorate<TService>(this IServiceCollection services, Func<TService, TService> decorator)
    {
        var descriptors = services.GetDescriptors<TService>();

        foreach (var descriptor in descriptors)
        {
            services.Replace(descriptor.Decorate(decorator));
        }

        return services;
    }

    private static List<ServiceDescriptor> GetDescriptors<TService>(this IServiceCollection services)
    {
        var descriptors = new List<ServiceDescriptor>();

        foreach (var service in services)
        {
            if (service.ServiceType == typeof(TService))
            {
                descriptors.Add(service);
            }
        }

        if (descriptors.Count == 0)
        {
            throw new InvalidOperationException($"Could not find any registered services for type '{typeof(TService).FullName}'.");
        }

        return descriptors;
    }

    private static ServiceDescriptor Decorate<TService>(this ServiceDescriptor descriptor, Func<TService, IServiceProvider, TService> decorator)
    {
        return descriptor.WithFactory(provider => decorator((TService) descriptor.GetInstance(provider), provider));
    }

    private static ServiceDescriptor Decorate<TService>(this ServiceDescriptor descriptor, Func<TService, TService> decorator)
    {
        return descriptor.WithFactory(provider => decorator((TService) descriptor.GetInstance(provider)));
    }

    private static ServiceDescriptor WithFactory(this ServiceDescriptor descriptor, Func<IServiceProvider, object> factory)
    {
        return ServiceDescriptor.Describe(descriptor.ServiceType, factory, descriptor.Lifetime);
    }

    private static object GetInstance(this ServiceDescriptor descriptor, IServiceProvider provider)
    {
        if (descriptor.ImplementationInstance != null)
        {
            return descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationType != null)
        {
            return provider.GetServiceOrCreateInstance(descriptor.ImplementationType);
        }

        return descriptor.ImplementationFactory(provider);
    }

    private static object GetServiceOrCreateInstance(this IServiceProvider provider, Type type)
    {
        return ActivatorUtilities.GetServiceOrCreateInstance(provider, type);
    }
}
}
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using CHC.Consent.WebApi.Abstractions;
using CHC.Consent.WebApi.Features.Person;
using CHC.Consent.WebApi.Infrastructure;
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
using CHC.Consent.NHibernate;
using CHC.Consent.NHibernate.Configuration;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.NHibernate.WebApi;
using CHC.Consent.Security;
using Swashbuckle.AspNetCore.SwaggerGen;

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
                        c.SwaggerDoc(
                            description.GroupName,
                            new Info
                            {
                                Version = description.GroupName,
                                Title = "CHC Consent API " + description.GroupName,
                                
                            });
                    }

                    c.TagActionsBy(description => description.GroupName);
                    c.OperationFilter<RemoveVersionPrefixFilter>();
                });

            services.AddSingleton<ISessionFactory>(
                new Configuration(
                    NHibernate.Configuration.Configuration.SqlServer(Configuration.GetConnectionString("Consent")))
            );

            services.AddScoped(s => new UnitOfWork(s.GetRequiredService<ISessionFactory>()));

            services.AddScoped<Func<global::NHibernate.ISession>>(
                s => () => s.GetRequiredService<UnitOfWork>().GetSession());

            services.AddTransient<IJwtIdentifiedUserRepository, UserRepository>();
            services.AddTransient<IPersonRepository, SecurePersonRepository>();
            
            services.AddTransient<IUserAccessor, HttpContextUserAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            
            app.UseAuthentication();
            app.UseMvc();
        }
    }

    public class RemoveVersionPrefixFilter : IOperationFilter
    {
        private static readonly int VersionPrefixLength = VersionPrefix.Length;
        private const string VersionPrefix = "V{version";

        /// <inheritdoc />
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.OperationId = operation.OperationId.StartsWith(VersionPrefix)
                ? operation.OperationId.Substring(VersionPrefixLength)
                : operation.OperationId;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using CHC.Consent.WebApi.Abstractions;
using CHC.Consent.WebApi.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using CHC.Consent.NHibernate;
using CHC.Consent.NHibernate.Configuration;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.NHibernate.WebApi;
using CHC.Consent.Security;
using CHC.Consent.Utils;
using CHC.Consent.WebApi.Abstractions.Consent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;
using NHibernate;
using Swashbuckle.AspNetCore.SwaggerGen;
using ISession = NHibernate.ISession;
using MicrosoftLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using ISessionFactory = CHC.Consent.NHibernate.ISessionFactory;

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
            services.AddLogging();
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

            services.AddMvc().AddXmlSerializerFormatters();

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
                    c.OperationFilter<AddSecurityFilter>();
                    c.AddSecurityDefinition("oidc", new OAuth2Scheme{});
                    var filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Api.xml");
                    c.IncludeXmlComments(filePath);
                });


            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            LoggerProvider.SetLoggersFactory(
                new LoggerFactoryAdapter(services.BuildServiceProvider().GetService<MicrosoftLoggerFactory>()));

            services.AddSingleton<IClock, UtcSystemClock>();

            services.AddSingleton<ISessionFactory>(
                new Configuration(
                    NHibernate.Configuration.Configuration.SqlServer(Configuration.GetConnectionString("Consent")))
            );

            services.AddScoped(s => new UnitOfWork(s.GetRequiredService<ISessionFactory>()));
            services.AddScoped<Func<ISession>>(s => () => s.GetRequiredService<UnitOfWork>().GetSession());

            services.AddScoped<IJwtIdentifiedUserRepository, UserRepository>();
            services.AddTransient<IPersonRepository, PersonRepository>();
            services.AddTransient<ISubjectStore, SubjectStore>();
            services.AddTransient<SecurityHelper>();
            
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
            app.Use(async (ctx, next) =>
            {
                using (var tx = ctx.RequestServices.GetService<UnitOfWork>().GetSession().BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        await next();
                        tx.Commit();
                    }
                    catch(Exception e) 
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            });

            app.UseMvc();



        }
    }

    public class AddSecurityFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.Security = new List<IDictionary<string, IEnumerable<string>>>
            {
                new Dictionary<string, IEnumerable<string>>
                {
                    {"oauth2", new string[] { }}
                }
            };
        }
    }
}
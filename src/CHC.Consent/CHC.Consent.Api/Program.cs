using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Identity;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Security;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CHC.Consent.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)                
                .ConfigureServices(ConfigureServices)
                .UseStartup<Startup>()
                .Build();

        private static readonly string MigrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

        public static void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;
            var environment = context.HostingEnvironment;

            services.AddIdentity<ConsentUser, ConsentRole>()
                .AddEntityFrameworkStores<ConsentContext>()
                .AddDefaultTokenProviders();

            services.AddDbContext<ConsentIdentityDbContext>(
                o => o.UseSqlServer(configuration.GetConnectionString("Users")));
           
            var id4 = services.AddIdentityServer(
                    options =>
                    {
                        options.Events.RaiseErrorEvents = true;
                        options.Events.RaiseInformationEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseSuccessEvents = true;
                    })
                .AddConfigurationStore(
                    options =>
                    {
                        options.ConfigureDbContext = builder =>
                            builder.UseSqlServer(
                                configuration.GetConnectionString("IdentityServer.Config"),
                                sql => sql.MigrationsAssembly(MigrationsAssembly));
                    })
                .AddOperationalStore(
                    options => options.ConfigureDbContext =
                        builder =>
                            builder.UseSqlServer(
                                configuration.GetConnectionString("IdentityServer.Grants"),
                                sql => sql.MigrationsAssembly(MigrationsAssembly))
                )
                .AddAspNetIdentity<ConsentUser>();

            if (environment.IsDevelopment())
            {
                id4.AddDeveloperSigningCredential();
            }
            else
            {
                throw new NotImplementedException("Need to configure keys for Identity Services");
            }

            var identityServerConfiguration = configuration.GetIdentityServer();
            services
                .AddAuthentication()
                .AddIdentityServerAuthentication(
                    options =>
                    {
                        options.Authority = identityServerConfiguration.Authority;
                        if (environment.IsDevelopment())
                            options.RequireHttpsMetadata = false;
                        options.ApiName = "api";
                    });
        }
    }
}
using System;
using System.Net.Mail;
using System.Reflection;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Security;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

            services.Configure<SmtpEmailSenderOptions>(configuration.GetSection("Smtp"));
            services.AddTransient<IEmailSender, SmtpEmailSender>();
            services.AddSingleton<IPostConfigureOptions<SmtpEmailSenderOptions>, FluentEmailConfiguration>();

            services.AddIdentity<ConsentUser, ConsentRole>()
                .AddEntityFrameworkStores<ConsentContext>()
                .AddDefaultTokenProviders();

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
        
        public class FluentEmailConfiguration : IPostConfigureOptions<SmtpEmailSenderOptions>
        {
            /// <inheritdoc />
            public void PostConfigure(string name, SmtpEmailSenderOptions options)
            {
                FluentEmail.Core.Email.DefaultSender =
                    new FluentEmail.Smtp.SmtpSender(() => new SmtpClient(options.Host, options.Port));
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CHC.Consent.Api.Infrastructure.Identity;
using CHC.Consent.EFCore.Security;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CHC.Consent.Api.Bootstrap
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            var host = BuildWebHost(args);

            using (var serviceScope = host.Services.CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                foreach (var client in Clients())
                {
                    if (db.Clients.All(_ => _.ClientId != client.ClientId))
                    {
                        db.Clients.Add(client.ToEntity());
                    }
                }

                foreach (var resource in IdentityResources())
                {
                    if (db.IdentityResources.All(_ => _.Name != resource.Name))
                    {
                        db.IdentityResources.Add(resource.ToEntity());
                    }
                }

                foreach (var api in Apis())
                {
                    if (db.ApiResources.All(_ => _.Name != api.Name))
                    {
                        db.ApiResources.Add(api.ToEntity());
                    }

                }

                db.SaveChanges();


                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<ConsentRole>>();
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ConsentUser>>();

                await roleManager.CreateAsync(new ConsentRole {Name = "BiB4All Study Manager"});

                await userManager.CreateAsync(new ConsentUser {UserName = "alice"}, "Pass123$");
                var consentUser = await userManager.FindByNameAsync("alice");
                await userManager.AddToRoleAsync(consentUser, "BiB4All Study Manager");
                await userManager.CreateAsync(new ConsentUser {UserName = "bob"}, "Pass123$");
            }
        }

        static IEnumerable<ApiResource> Apis()
        {
            yield return new ApiResource {Name = "api", DisplayName = "Consent API Services"};
        }
        

        static IEnumerable<IdentityResource> IdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }
        

        static IEnumerable<Client> Clients()
        {
            yield return new Client
            {
                ClientId = "UI",
                ClientName = "User Interface",

                AllowedGrantTypes = GrantTypes.Implicit,
                ClientSecrets = {new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256())},

                RedirectUris = {"http://localhost:5000/signin-oidc"},
                FrontChannelLogoutUri = "http://localhost:5000/signout-oidc",
                PostLogoutRedirectUris = {"http://localhost:5000/signout-callback-oidc"},
                RequireConsent = true,

                AllowOfflineAccess = true,
                AllowedScopes = {"openid", "profile", "api"}
            };
        }

        private static IWebHost BuildWebHost(string[] args) => Api.Program.BuildWebHost(args);
    }
}
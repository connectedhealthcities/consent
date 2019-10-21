using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.EFCore.Security;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CHC.Consent.Api.Bootstrap
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            var host = BuildWebHost(args);

            var services = host.Services;
            using (var serviceScope = services.CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                foreach (var client in Clients())
                {

                    var existingId = db.Clients.Where(_ => _.ClientId == client.ClientId).Select(_ => (int?) _.Id)
                        .FirstOrDefault();
                    if (existingId == null)
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

                var bib4allStudyManager = "BiB4All Study Manager";
                await roleManager.CreateAsync(new ConsentRole {Name = bib4allStudyManager});

                await roleManager.CreateAsync(new ConsentRole {Name = "Website Admin"});

                await userManager.CreateAsync(new ConsentUser {UserName = "alice"}, "Pass123$");
                var consentUser = await userManager.FindByNameAsync("alice");
                await userManager.AddToRoleAsync(consentUser, bib4allStudyManager);
                await userManager.CreateAsync(new ConsentUser {UserName = "bob"}, "Pass123$");

                using (var consent = serviceScope.ServiceProvider.GetService<ConsentContext>())
                {
                    var bib4All =
                        consent.Studies.Include(_ => _.ACL).SingleOrDefault(_ => _.Name == "BiB4All")
                        ?? consent.Studies.Add(new StudyEntity {Name = "BiB4All"}).Entity;
                    
                    var permissionEntities = consent.Set<PermissionEntity>();
                    foreach (var permission in Permissions)
                    {
                        if (!permissionEntities.Any(_ => _.Access == permission))
                            permissionEntities.Add(new PermissionEntity {Access = permission});
                    }
                    
                    consent.SaveChanges();

                    foreach (var role in consent.Roles.Where(_ => _.Name == bib4allStudyManager))
                    {
                        consent.GrantPermission(bib4All, role, PermissionNames.Read);
                    }

                    foreach (var identifier in Identifiers)
                    {
                        var entities = consent.IdentifierDefinition;
                        if (entities.All(_ => _.Name != identifier.Name))
                        {
                            entities.Add(identifier);
                        }
                    }

                    consent.EvidenceDefinition.AddRange(
                        KnownEvidence.Registry.Cast<EvidenceDefinition>()
                            .Select(_ => new EvidenceDefinitionEntity(_.Name, _.SystemName))
                            .Where(e => consent.EvidenceDefinition.All(ent => ent.Name != e.Name))
                    );

                    foreach (var authority in Authorities)
                    {
                        if(!consent.Set<AuthorityEntity>().Any(_ => _.SystemName == authority.SystemName))
                        {
                            consent.Add(authority);
                        }
                    }

                    consent.SaveChanges();
                }
            }

        }

        private static IEnumerable<AuthorityEntity> Authorities
        {
            get
            {
                yield return new AuthorityEntity("Medway", 150, "medway");
                yield return new AuthorityEntity("Bradford PDS", 10, "bradford-pds");
            }
        }


        private static IEnumerable<string> Permissions { get; } =
            typeof(PermissionNames)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(_ => _.FieldType == typeof(string))
                .Select(f => (string) f.GetValue(null));

        static IEnumerable<ApiResource> Apis()
        {
            yield return new ApiResource
            {
                Name = "api", 
                DisplayName = "Consent API Services",
                Scopes = { new Scope("read"), new Scope("write"), new Scope("api") }
            };
        }
        

        static IEnumerable<IdentityResource> IdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(), 
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
            
            yield return new Client
            {
                ClientId = "ApiExplorer",
                ClientName = "Api User Interface",

                AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials,
                ClientSecrets = {new Secret("20672b51-679f-4de7-b6f8-810794afa6ca".Sha256())},
                RequireClientSecret = false,
                AllowAccessTokensViaBrowser = true,
                
                RedirectUris = {"http://localhost:5000/swagger/oauth2-redirect.html"},
                
                RequireConsent = false,

                AllowOfflineAccess = true,
                AllowedScopes = {"openid", "profile", "api", "read", "write" }
            };
            
        }
        
        static readonly IEnumerable<IdentifierDefinitionEntity> Identifiers = new []
        {
            new IdentifierDefinitionEntity ( "NHS Number", "nhs-number:string"),
            new IdentifierDefinitionEntity ( "Sex", "sex:enum('Female','Male')"),
            new IdentifierDefinitionEntity ( "Date of Birth", "date-of-birth:date"),
            new IdentifierDefinitionEntity ( "Bradford Hospital Number", "bradford-hospital-number:string"),
            new IdentifierDefinitionEntity(
                "Address", 
                @"address:composite(line-1:string,line-2:string,line-3:string,line-4:string,line-5:string,postcode:string)"),
            new IdentifierDefinitionEntity("Birth Order", "birth-order:composite(pregnancy-number:integer,birth-order:string)"),
            new IdentifierDefinitionEntity("Name", "name:composite(given:string,family:string)"),
            new IdentifierDefinitionEntity("Contact Number", "contact-number:composite(type:string,number:string)"),
            
        };

        private static IWebHost BuildWebHost(string[] args) => Api.Program.BuildWebHost(args);
    }
}
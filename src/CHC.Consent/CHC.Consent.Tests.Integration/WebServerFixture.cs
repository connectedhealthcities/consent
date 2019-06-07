using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CHC.Consent.Api;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Testing.Utils;
using CHC.Consent.Tests.Api.Client;
using IdentityModel.Client;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Xunit.Abstractions;

namespace CHC.Consent.Tests
{
    public class WebServerFixture : IDisposable
    {
        private TestOutputLoggerProvider LoggerProvider;
        private XUnitServiceClientTracingInterceptor tracingInterceptor;
        
        // used as type parameter
        // ReSharper disable once ClassNeverInstantiated.Local
        private class InMemoryDatabaseStartup : Startup
        {
            /// <inheritdoc />
            public InMemoryDatabaseStartup(IConfiguration configuration, IHostingEnvironment environment) : 
                base(configuration, environment)
            {
            }

            /// <inheritdoc />
            protected override void ConfigureDatabaseOptions(IServiceProvider provider, DbContextOptionsBuilder options)
            {
                options.ConfigureWarnings(_ => _.Default(WarningBehavior.Ignore));
                options.EnableSensitiveDataLogging();


                options.UseSqlServer(
                    $@"Server=(localdb)\MSSqlLocalDB;Integrated Security=true;Initial Catalog=ChCIntegrations");
            }
        }

        public ITestOutputHelper Output
        {
            set
            {
                LoggerProvider.Output = value;
                tracingInterceptor.Output = value;
            }
        }

        public TestServer Server { get; }

        public HttpClient Client { get; }
        
        public Consent.Api.Client.Api ApiClient { get; }

        /// <inheritdoc />
        public WebServerFixture()
        {
            LoggerProvider = new TestOutputLoggerProvider();
            
            var identityServer = new TestServer(
                new WebHostBuilder()
                //.ConfigureLogging(_ => _.AddProvider(new TestOutputLoggerProvider()))
                .Configure(app =>
                    {
                        app.UseIdentityServer();
                    })
                .ConfigureServices(
                        (context, services) =>
                        {
                            services.AddIdentityServer()
                                .AddDeveloperSigningCredential()
                                .AddInMemoryPersistedGrants()
                                .AddInMemoryIdentityResources(
                                    new IdentityResource[]
                                        {new IdentityResources.OpenId(), new IdentityResources.Profile(),})
                                .AddInMemoryApiResources(
                                    new[]
                                    {
                                        new ApiResource("api", "The API")
                                        {
                                            ApiSecrets = {new Secret("secret".Sha256())},
                                            /*Scopes = { new Scope("api")}*/
                                        }
                                    })
                                .AddInMemoryClients(
                                    new[]
                                    {
                                        new Client
                                        {
                                            ClientId = "client",
                                            AllowedGrantTypes = GrantTypes.ClientCredentials,
                                            ClientSecrets =
                                            {
                                                new Secret("secret".Sha256())
                                            },
                                            AllowedScopes =
                                            {
                                                IdentityServerConstants.StandardScopes.OpenId,
                                                IdentityServerConstants.StandardScopes.Profile,
                                                "api"
                                            },
                                            AllowOfflineAccess = true
                                        }
                                    })
                                .AddTestUsers(new List<TestUser>());
                                
                        } )
                );


            Server = new TestServer(
                new WebHostBuilder()
                    .UseStartup<InMemoryDatabaseStartup>()
                    .ConfigureLogging(_ => _.AddProvider(LoggerProvider).SetMinimumLevel(LogLevel.Trace))
                    .ConfigureAppConfiguration(
                        b => b.AddInMemoryCollection(
                            new Dictionary<string, string> {["IdentityServer:EnableInternalServer"] = false.ToString()}))
                    .ConfigureServices(
                        services =>
                        {
                            services.AddAuthentication().AddIdentityServerAuthentication(
                                options =>
                                {
                                    options.ApiSecret = "secret";
                                    options.ApiName = "api";
                                    options.IntrospectionDiscoveryHandler = identityServer.CreateHandler();
                                    options.IntrospectionBackChannelHandler = identityServer.CreateHandler();
                                    options.JwtBackChannelHandler = identityServer.CreateHandler();
                                    options.RequireHttpsMetadata = false;
                                    options.Authority = identityServer.BaseAddress.ToString();
                                    
                                    options.Validate();
                                });
                        })
            );

            
            
            Client = Server.CreateClient();
            Client.GetAsync("/");

            var discoveryClient = new DiscoveryClient(identityServer.BaseAddress.ToString(), identityServer.CreateHandler());
            var discoveryResponse = discoveryClient.GetAsync().GetAwaiter().GetResult();
            if(discoveryResponse.IsError) throw new Exception(discoveryResponse.Error);
            
            var client = new TokenClient(
                discoveryResponse.TokenEndpoint,
                "client",
                "secret",
                innerHttpMessageHandler: identityServer.CreateHandler());

            var response = client.RequestClientCredentialsAsync(scope:"api").GetAwaiter().GetResult();

            Client.SetBearerToken(response.AccessToken);

            ApiClient = new Consent.Api.Client.Api(
                Client,
                new TokenCredentials(response.AccessToken),
                disposeHttpClient: false);
                   
            
            
            ServiceClientTracing.IsEnabled = true;
            tracingInterceptor = new XUnitServiceClientTracingInterceptor(null);
            ServiceClientTracing.AddTracingInterceptor(tracingInterceptor);

            
        }

        public class TestOutputLoggerProvider : ILoggerProvider
        {
            private ITestOutputHelper output;

            public ITestOutputHelper Output
            {
                get => output;
                set
                {
                    output = value;
                    if (value != null)
                    {
                        LoggerQueue.DumpTo(output.WriteLine);
                    }
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                
            }

            /// <inheritdoc />
            public ILogger CreateLogger(string categoryName)
            {
                return Output == null ? (ILogger) new LoggerQueue(categoryName) : new XunitLogger(Output, categoryName);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Server?.Dispose();
            Client?.Dispose();
        }

        public T AddData<T>(Func<ConsentContext,T> add)
        {
            using (var scope = Server.Host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ConsentContext>();
                var result = add(context);
                context.SaveChanges();
                return result;
            }
        }
    }
}
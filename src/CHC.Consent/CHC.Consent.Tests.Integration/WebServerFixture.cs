using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CHC.Consent.Api;
using CHC.Consent.Testing.Utils;
using CHC.Consent.Tests.Api.Client;
using IdentityModel.Client;
using IdentityServer4;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        private class InMemorydatabaseStartup : Startup
        {
            /// <inheritdoc />
            public InMemorydatabaseStartup(IConfiguration configuration, IHostingEnvironment environment) : 
                base(configuration, environment)
            {
            }

            /// <inheritdoc />
            protected override void ConfigureDatabaseOptions(IServiceProvider provider, DbContextOptionsBuilder options)
            {
                options.UseInMemoryDatabase("CHC-Testing");
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
                .ConfigureLogging(_ => _.AddProvider(LoggerProvider))
                .Configure(app =>
                    {
                        app.UseIdentityServer();
                        app.UseMvc();
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
                                        new ApiResource("api", "The API") {ApiSecrets = {new Secret("secret".Sha256())}}
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
                                
                            services.AddMvc();
                        } )
                );


            Server = new TestServer(
                new WebHostBuilder()
                    .UseStartup<InMemorydatabaseStartup>()
                    .ConfigureLogging(_ => _.AddProvider(LoggerProvider))
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

            var discoveryClient = new DiscoveryClient(identityServer.BaseAddress.ToString(), identityServer.CreateHandler());
            var discoveryResponse = discoveryClient.GetAsync().GetAwaiter().GetResult();
            
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

            public TestOutputLoggerProvider()
            {
                
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
    }
}
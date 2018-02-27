using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.CompilerServices;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Common;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.Testing.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Rest;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Sex = CHC.Consent.Api.Client.Models.Sex;

namespace CHC.Consent.Tests.Api.Client
{
    using Random = Testing.Utils.Random;
    using Api = CHC.Consent.Api.Client.Api;
    [Collection(WebServerCollection.Name)]
    public class ApiClientTests : IDisposable
    {
        private XUnitServiceClientTracingInterceptor tracingInterceptor;
        public ITestOutputHelper Output { get; }
        public WebServerFixture Fixture { get; }

        /// <inheritdoc />
        public ApiClientTests(ITestOutputHelper output, WebServerFixture fixture)
        {
            Output = output;
            Fixture = fixture;
            ServiceClientTracing.IsEnabled = true;
            tracingInterceptor = new XUnitServiceClientTracingInterceptor(Output);
            ServiceClientTracing.AddTracingInterceptor(tracingInterceptor);
        }

        [Theory, MemberData(nameof(IdentityTestData))]
        public async void CanSendIdentitiesToServer(IIdentifier identifier, Predicate<PersonEntity> check)
        {
            var client = new Api(Fixture.Client, disposeHttpClient:false);
            var api = (IApi) client;

            
            var response = await api.IdentitiesPutWithHttpMessagesAsync(
                new PersonSpecification(
                    new List<IIdentifier> {identifier},
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IIdentifier> {identifier}}
                    }));

            var peopleStore = Fixture.Server.Host.Services.GetService<ConsentContext>().People;

            Assert.Single(peopleStore, check);
        }

        public static IEnumerable<object[]> IdentityTestData =>
            MakeTestData(
                NhsNumberTestData(),
                DateOfBirthTestData(),
                SexMale(),
                SexFemale()
            );
    

        private static IEnumerable<object[]> MakeTestData(params (IIdentifier, Predicate<PersonEntity>)[] tests)
        {
            IEnumerable<object> ToEnumerable(ITuple tuple)
            {
                for (var i = 0; i < tuple.Length; i++)
                {
                    yield return tuple[i];
                }
            }
            
            return tests.Cast<ITuple>().Select(ToEnumerable).Select(Enumerable.ToArray);
        }
        
        private static (IIdentifier, Predicate<PersonEntity>) NhsNumberTestData()
        {
            var nhsNumber = Random.String();
            return (new UknhsnhsNumber(nhsNumber), person => person.NhsNumber == nhsNumber);
        }

        private static (IIdentifier, Predicate<PersonEntity>) DateOfBirthTestData()
        {
            DateTime? date = 24.April(1865);
            return (new DateOfBirth(date), _ => _.DateOfBirth == date);
        }

        private static (IIdentifier, Predicate<PersonEntity>) SexMale()
        {
            var sex = new Sex("Male");
            return (sex, p => p.Sex == Common.Sex.Male);
        }
        
        private static (IIdentifier, Predicate<PersonEntity>) SexFemale()
        {
            var sex = new Sex(Common.Sex.Female.ToString());
            return (sex, p => p.Sex == Common.Sex.Female);
        }

        
        
        /// <inheritdoc />
        public void Dispose()
        {
            ServiceClientTracing.RemoveTracingInterceptor(tracingInterceptor);
        }
    }

    internal class XUnitServiceClientTracingInterceptor : IServiceClientTracingInterceptor
    {
        public ITestOutputHelper Output { get; }

        /// <inheritdoc />
        public XUnitServiceClientTracingInterceptor(ITestOutputHelper output)
        {
            Output = output;
        }

        /// <inheritdoc />
        public void Configuration(string source, string name, string value)
        {
            Output.WriteLine("Configuration: source:{0} name:{1} value:{2}", source, name, value);
        }

        /// <inheritdoc />
        public void EnterMethod(string invocationId, object instance, string method, IDictionary<string, object> parameters)
        {
            
        }

        /// <inheritdoc />
        public void ExitMethod(string invocationId, object returnValue)
        {
            
        }

        /// <inheritdoc />
        public void Information(string message)
        {
            Output.WriteLine("Information: {0}", message);
        }

        /// <inheritdoc />
        public void ReceiveResponse(string invocationId, HttpResponseMessage response)
        {
            Output.WriteLine("Response: InvocationId:{0} {1}",invocationId, response.AsFormattedString());
        }

        /// <inheritdoc />
        public void SendRequest(string invocationId, HttpRequestMessage request)
        {
            
            Output.WriteLine("Response: InvocationId:{0} {1}", invocationId, request.AsFormattedString());
        }

        /// <inheritdoc />
        public void TraceError(string invocationId, Exception exception)
        {
            Output.WriteLine("Error: InvocationId:{0} {1}", invocationId, exception);
        }
    }
}
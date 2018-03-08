using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using Sex = CHC.Consent.Api.Client.Models.UkNhsBradfordhospitalsBib4allMedwaySex;

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
        public void CanSendIdentitiesToServer(IPersonIdentifier identifier, Action<IPersonIdentifier> checkResult)
        {
            var client = new Api(Fixture.Client, disposeHttpClient:false);
            var api = (IApi) client;

            
            var response = api.IdentitiesPut(
                new PersonSpecification(
                    new List<IPersonIdentifier> {identifier},
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IPersonIdentifier> {identifier}}
                    }));

            Assert.NotNull(response);
            
            var identities = api.IdentitiesByIdGet(response.Value);

            Output.WriteLine(string.Join("\n", identities.Select(_ => _.ToString())));

            checkResult(Assert.Single(identities));
        }

        public static IEnumerable<object[]> IdentityTestData =>
            MakeTestData(
                NhsNumberTestData(),
                DateOfBirthTestData(),
                SexMale(),
                SexFemale(),
                MedwayName()
            );

        private static (IPersonIdentifier, Action<IPersonIdentifier>) MedwayName()
        {
            var firstName = Random.String();
            var lastName = Random.String();
            return (
                new UkNhsBradfordhospitalsBib4allMedwayName(firstName, lastName), 
                i =>
                {
                    var otherName = Assert.IsType<UkNhsBradfordhospitalsBib4allMedwayName>(i);
                    Assert.Equal(firstName, otherName.FirstName);
                    Assert.Equal(lastName, otherName.LastName);
                }
            );
        }


        private static IEnumerable<object[]> MakeTestData(params (
            IPersonIdentifier, 
            Action<IPersonIdentifier>)[] tests)
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
        
        private static (IPersonIdentifier, Action<IPersonIdentifier>) NhsNumberTestData()
        {
            var nhsNumber = Random.String();
            return (
                new UkNhsNhsNumber(nhsNumber), 
                i => Assert.Equal(nhsNumber, Assert.IsType<UkNhsNhsNumber>(i).Value));
        }

        private static (IPersonIdentifier, Action<IPersonIdentifier>) DateOfBirthTestData()
        {
            DateTime date = 24.April(1865);
            return (
                new UkNhsBradfordhospitalsBib4allMedwayDateOfBirth(date), 
                i => Assert.Equal(
                    date.ToLocalTime(),
                    Assert.IsType<UkNhsBradfordhospitalsBib4allMedwayDateOfBirth>(i).DateOfBirth.ToLocalTime())
            );
        }

        private static (IPersonIdentifier, Action<IPersonIdentifier>) SexMale()
        {
            var sex = new Sex("Male");
            return (
                sex, 
                i => Assert.Equal("Male", Assert.IsType<Sex>(i).Sex));
        }
        
        private static (IPersonIdentifier, Action<IPersonIdentifier>) SexFemale()
        {
            var sex = new Sex(Common.Sex.Female.ToString());
            return (
                sex, 
                i => Assert.Equal("Female", Assert.IsType<Sex>(i).Sex));
        }
        
        [Fact]
        public void CanSendMultipleIdentitiesToServer()
        {
            var client = new Api(Fixture.Client, disposeHttpClient:false);
            var api = (IApi) client;

            var nhsNumber = new UkNhsNhsNumber("4334443434");
            var medwayName = new UkNhsBradfordhospitalsBib4allMedwayName("Rachel", "Thompson");
            
            
            var response = api.IdentitiesPut(
                new PersonSpecification(
                    new List<IPersonIdentifier> {nhsNumber, medwayName},
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IPersonIdentifier> {nhsNumber}}
                    }));

            Assert.NotNull(response);
            
            var identities = api.IdentitiesByIdGet(response.Value);

            Assert.Equal(nhsNumber.Value, Assert.Single(identities.OfType<UkNhsNhsNumber>()).Value);
            var storedMedwayName = Assert.Single(identities.OfType<UkNhsBradfordhospitalsBib4allMedwayName>());
            Assert.NotNull(storedMedwayName);
            Assert.Equal(medwayName.FirstName, storedMedwayName.FirstName);
            Assert.Equal(medwayName.LastName, storedMedwayName.LastName);
        }

        
        
        /// <inheritdoc />
        public void Dispose()
        {
            ServiceClientTracing.RemoveTracingInterceptor(tracingInterceptor);
        }
    }
}
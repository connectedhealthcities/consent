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
        public void CanSendIdentitiesToServer(IIdentifier identifier, Action<IIdentifier> checkResult)
        {
            var client = new Api(Fixture.Client, disposeHttpClient:false);
            var api = (IApi) client;

            
            var response = api.IdentitiesPut(
                new PersonSpecification(
                    new List<IIdentifier> {identifier},
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IIdentifier> {identifier}}
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

        private static (IIdentifier, Action<IIdentifier>) MedwayName()
        {
            var firstName = Random.String();
            var lastName = Random.String();
            return (
                new Uknhsbradfordhospitalsbib4allmedwayname(firstName, lastName), 
                i =>
                {
                    var otherName = Assert.IsType<Uknhsbradfordhospitalsbib4allmedwayname>(i);
                    Assert.Equal(firstName, otherName.FirstName);
                    Assert.Equal(lastName, otherName.LastName);
                }
            );
        }


        private static IEnumerable<object[]> MakeTestData(params (
            IIdentifier, 
            Action<IIdentifier>)[] tests)
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
        
        private static (IIdentifier, Action<IIdentifier>) NhsNumberTestData()
        {
            var nhsNumber = Random.String();
            return (
                new UknhsnhsNumber(nhsNumber), 
                i => Assert.Equal(nhsNumber, Assert.IsType<UknhsnhsNumber>(i).Value));
        }

        private static (IIdentifier, Action<IIdentifier>) DateOfBirthTestData()
        {
            DateTime? date = 24.April(1865);
            return (
                new DateOfBirth(date),
                i => Assert.Equal(
                    date.Value.ToLocalTime(),
                    Assert.IsType<DateOfBirth>(i).DateOfBirthProperty?.ToLocalTime())
            );
        }

        private static (IIdentifier, Action<IIdentifier>) SexMale()
        {
            var sex = new Sex("Male");
            return (
                sex, 
                i => Assert.Equal("Male", Assert.IsType<Sex>(i).SexProperty));
        }
        
        private static (IIdentifier, Action<IIdentifier>) SexFemale()
        {
            var sex = new Sex(Common.Sex.Female.ToString());
            return (
                sex, 
                i => Assert.Equal("Female", Assert.IsType<Sex>(i).SexProperty));
        }
        
        [Fact]
        public void CanSendMultipleIdentitiesToServer()
        {
            var client = new Api(Fixture.Client, disposeHttpClient:false);
            var api = (IApi) client;

            var nhsNumber = new UknhsnhsNumber("4334443434");
            var medwayName = new Uknhsbradfordhospitalsbib4allmedwayname("Rachel", "Thompson");
            
            
            var response = api.IdentitiesPut(
                new PersonSpecification(
                    new List<IIdentifier> {nhsNumber, medwayName},
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IIdentifier> {nhsNumber}}
                    }));

            Assert.NotNull(response);
            
            var identities = api.IdentitiesByIdGet(response.Value);

            Assert.Equal(nhsNumber.Value, Assert.Single(identities.OfType<UknhsnhsNumber>()).Value);
            var storedMedwayName = Assert.Single(identities.OfType<Uknhsbradfordhospitalsbib4allmedwayname>());
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
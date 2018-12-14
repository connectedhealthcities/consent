﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
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
using IPersonIdentifier = CHC.Consent.Api.Client.Models.IPersonIdentifier;
using Sex = CHC.Consent.Api.Client.Models.Sex;

namespace CHC.Consent.Tests.Api.Client
{
    using Random = Testing.Utils.Random;
    using Api = CHC.Consent.Api.Client.Api;
    [Collection(WebServerCollection.Name)]
    public class ApiClientTests
    {
        public ITestOutputHelper Output { get; }
        public WebServerFixture Fixture { get; }

        /// <inheritdoc />
        public ApiClientTests(ITestOutputHelper output, WebServerFixture fixture)
        {
            Output = output;
            Fixture = fixture;
            Fixture.Output = output;
            
        }

        [Theory, MemberData(nameof(IdentityTestData))]
        public void CanSendIdentitiesToServer(IPersonIdentifier identifier, Action<IPersonIdentifier> checkResult)
        {
            var response = Fixture.ApiClient.PutPerson(
                new PersonSpecification(
                    new List<IPersonIdentifier> {identifier},
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IPersonIdentifier> {identifier}}
                    }));

            Assert.NotNull(response);
            
            var identities = Fixture.ApiClient.GetPerson(response.PersonId);

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
                new Name(new NameValue(firstName, lastName)), 
                i =>
                {
                    var otherName = Assert.IsType<Name>(i);
                    Assert.Equal(firstName, otherName.Value?.FirstName);
                    Assert.Equal(lastName, otherName.Value?.LastName);
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
                new NhsNumber(nhsNumber), 
                i => Assert.Equal(nhsNumber, Assert.IsType<NhsNumber>(i).Value));
        }

        private static (IPersonIdentifier, Action<IPersonIdentifier>) DateOfBirthTestData()
        {
            DateTime date = 24.April(1865);
            return (
                new DateOfBirth(date), 
                i => Assert.Equal(
                    date.ToLocalTime(),
                    Assert.IsType<DateOfBirth>(i).Value)
            );
        }

        private static (IPersonIdentifier, Action<IPersonIdentifier>) SexMale()
        {
            var sex = new Sex("Male");
            return (
                sex, 
                i => Assert.Equal("Male", Assert.IsType<Sex>(i).Value));
        }
        
        private static (IPersonIdentifier, Action<IPersonIdentifier>) SexFemale()
        {
            var sex = new Sex(Common.Sex.Female.ToString());
            return (
                sex, 
                i => Assert.Equal("Female", Assert.IsType<Sex>(i).Value));
        }
        
        [Fact]
        public void CanSendMultipleIdentitiesToServer()
        {
            
            var api = Fixture.ApiClient;

            var nhsNumber = new NhsNumber("4334443434");
            var medwayName = new Name(new NameValue("Rachel", "Thompson"));
            
            
            var response = api.PutPerson(
                new PersonSpecification(
                    new List<IPersonIdentifier> {nhsNumber, medwayName},
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IPersonIdentifier> {nhsNumber}}
                    }));

            Assert.NotNull(response);
            
            var identities = api.GetPerson(response.PersonId);

            Assert.Equal(nhsNumber.Value, Assert.Single(identities.OfType<NhsNumber>()).Value);
            var storedMedwayName = Assert.Single(identities.OfType<Name>());
            Assert.NotNull(storedMedwayName);
            Assert.Equal(medwayName.Value.FirstName, storedMedwayName.Value.FirstName);
            Assert.Equal(medwayName.Value.LastName, storedMedwayName.Value.LastName);
        }
    }
}
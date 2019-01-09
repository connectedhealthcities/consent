using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using IdentifierDefinition = CHC.Consent.Common.Identity.Identifiers.IdentifierDefinition;


namespace CHC.Consent.Tests.Integration.Api
{
    using Random = Testing.Utils.Random;

    [Collection(WebServerCollection.Name)]
    public class ApiClientTests
    {
        private ITestOutputHelper Output { get; }
        private WebServerFixture Fixture { get; }

        /// <inheritdoc />
        public ApiClientTests(ITestOutputHelper output, WebServerFixture fixture)
        {
            Output = output;
            Fixture = fixture;
            Fixture.Output = output;
        }

        [Theory, MemberData(nameof(IdentityTestData))]
        public void CanSendIdentitiesToServer(IdentifierValue identifier, Action<IdentifierValue> checkResult)
        {
            var response = Fixture.ApiClient.PutPerson(
                new PersonSpecification(
                    new List<IdentifierValue> {identifier},
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IdentifierValue> {identifier}}
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

        private static (IdentifierValue, Action<IdentifierValue>) MedwayName()
        {
            var (name, firstNameValue, lastNameValue) = Name(Random.String(), Random.String());
            return (
                name,
                i =>
                {
                    var nameParts = Assert.IsType<IdentifierValue[]>(i.Value);
                    nameParts.Should()
                        .Contain(Identifier(firstNameValue))
                        .And
                        .Contain(Identifier(lastNameValue))
                        .And
                        .HaveCount(2);
                }
            );
        }

        private static (IdentifierValue name, IdentifierValue fistName, IdentifierValue lastName) Name(string firstName, string lastName)
        {
            var firstNameValue = Value(Identifiers.Definitions.FirstName, firstName);
            var lastNameValue = Value(Identifiers.Definitions.LastName, lastName);
            var name = Value(
                Identifiers.Definitions.Name,
                new[]
                {
                    firstNameValue,
                    lastNameValue,
                });
            return (name, firstNameValue, lastNameValue);
        }

        private static IdentifierValue Value(IdentifierDefinition identifierDefinition, object value)
        {
            return new IdentifierValue {Name = identifierDefinition.SystemName, Value = value};
        }

        private static Expression<Func<IdentifierValue, bool>> Identifier(IdentifierValue value)
        {
            return _ => _.Name == value.Name && Equals(_.Value, value.Value);
        }


        private static IEnumerable<object[]> MakeTestData(params (IdentifierValue,Action<IdentifierValue>)[] tests)
        {
            IEnumerable<object> ToEnumerable((IdentifierValue, Action<IdentifierValue>) tuple)
            {
                var (value, test) = tuple;
                yield return value;
                yield return test ?? (i => i.Should().Match(Identifier(value)));
            }

            return tests.Select(ToEnumerable).Select(Enumerable.ToArray);
        }
        
        private static (IdentifierValue, Action<IdentifierValue>) NhsNumberTestData()
        {
            var value = NhsNumber(Random.String());
            return (value,null);
        }

        private static IdentifierValue NhsNumber(string value)
        {
            return Value(Identifiers.Definitions.NhsNumber, value);
        }

        private static (IdentifierValue, Action<IdentifierValue>) DateOfBirthTestData()
        {
            return (Value(Identifiers.Definitions.DateOfBirth, 24.April(1865)), null);

        }

        private static (IdentifierValue, Action<IdentifierValue>) SexMale()
        {
            return (Value(Identifiers.Definitions.Sex, "Male"), null);
        }
        
        private static (IdentifierValue, Action<IdentifierValue>) SexFemale()
        {
            var sex = Value(Identifiers.Definitions.Sex, "Female");
            return (sex,null);
        }
        
        [Fact]
        public void CanSendMultipleIdentitiesToServer()
        {
            
            var api = Fixture.ApiClient;

            var nhsNumber = NhsNumber("4334443434");
            var (name, _, _) = Name(Random.String(), Random.String());
            
            
            var response = api.PutPerson(
                new PersonSpecification(
                    new List<IdentifierValue> {nhsNumber, name},
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IdentifierValue> {nhsNumber}}
                    }));

            Assert.NotNull(response);
            
            var identities = api.GetPerson(response.PersonId);

            identities.Should().Contain(Identifier(nhsNumber));
            
            
        }
    }
}
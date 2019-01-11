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
        public void CanSendIdentitiesToServer(IIdentifierValueDto identifier, Action<IIdentifierValueDto> checkResult)
        {
            PersonCreatedResult response=null;
            Fixture.ApiClient.Invoking(
                    _ => response  = _.PutPerson(
                        new PersonSpecification(
                            new List<IIdentifierValueDto> {identifier},
                            new List<MatchSpecification>
                            {
                                new MatchSpecification {Identifiers = new List<IIdentifierValueDto> {identifier}}
                            }))
                )
                .Should().NotThrow();
                
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
                Name()
            );

        private static (IIdentifierValueDto, Action<IIdentifierValueDto>) Name()
        {
            var (name, firstNameValue, lastNameValue) = Name(Random.String(), Random.String());
            return (
                name,
                i =>
                {
                    i.Should().BeOfType<IdentifierValueDtoIIdentifierValueDto>()
                        .Subject.Value.Should()
                        .Contain(Identifier(firstNameValue))
                        .And
                        .Contain(Identifier(lastNameValue))
                        .And
                        .HaveCount(2);
                }
            );
        }

        private static (IIdentifierValueDto name, IIdentifierValueDto fistName, IIdentifierValueDto lastName) Name(string firstName, string lastName)
        {
            var firstNameValue = Identifiers.Definitions.FirstName.Value(firstName);
            var lastNameValue = Identifiers.Definitions.LastName.Value(lastName);
            var name = 
                Identifiers.Definitions.Name.Value(
                new[]
                {
                    firstNameValue,
                    lastNameValue,
                });
            return (name, firstNameValue, lastNameValue);
        }

        

        private static Expression<Func<IIdentifierValueDto, bool>> Identifier(IIdentifierValueDto value)
        {
            return actual => HasSameName(actual, value) && HasSameValue(actual, value);
        }

        private static bool HasSameName(dynamic actual, dynamic value)
        {
            return Equals(actual.Name, value.Name);
        }

        private static bool HasSameValue(dynamic a, dynamic b)
        {
            return Equals(a.Value, b.Value);
        }


        private static IEnumerable<object[]> MakeTestData(params (IIdentifierValueDto,Action<IIdentifierValueDto>)[] tests)
        {
            IEnumerable<object> ToEnumerable((IIdentifierValueDto, Action<IIdentifierValueDto>) tuple)
            {
                var (value, test) = tuple;
                yield return value;
                yield return test ?? (i =>
                                 i.Should().BeOfType(value.GetType()).And.Subject.Should()
                                     .BeEquivalentTo(
                                     value,
                                     options => options.RespectingRuntimeTypes()));
            }

            return tests.Select(ToEnumerable).Select(Enumerable.ToArray);
        }
        
        private static (IIdentifierValueDto, Action<IIdentifierValueDto>) NhsNumberTestData()
        {
            var value = NhsNumber(Random.String());
            return AreEqual(value);
        }

        private static (IIdentifierValueDto, Action<IIdentifierValueDto>) AreEqual<T>(T value)
            where T : IIdentifierValueDto
        {
            return (value, i => i.Should().BeEquivalentTo(value, options => options.RespectingRuntimeTypes()));
        }

        private static IdentifierValueDtoString NhsNumber(string value)
        {
            return Identifiers.Definitions.NhsNumber.Value(value);
        }

        private static (IIdentifierValueDto, Action<IIdentifierValueDto>) DateOfBirthTestData()
        {
            return AreEqual(Identifiers.Definitions.DateOfBirth.Value(24.April(1865)));

        }

        private static (IIdentifierValueDto, Action<IIdentifierValueDto>) SexMale()
        {
            return AreEqual(Identifiers.Definitions.Sex.Value("Male"));
        }
        
        private static (IIdentifierValueDto, Action<IIdentifierValueDto>) SexFemale()
        {
            return AreEqual(Identifiers.Definitions.Sex.Value("Female"));
        }
        
        [Fact]
        public void CanSendMultipleIdentitiesToServer()
        {
            
            var api = Fixture.ApiClient;

            var nhsNumber = NhsNumber("4334443434");
            var (name, _, _) = Name(Random.String(), Random.String());
            
            
            var response = api.PutPerson(
                new PersonSpecification(
                    new List<IIdentifierValueDto> {nhsNumber, name},
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IIdentifierValueDto> {nhsNumber}}
                    }));

            Assert.NotNull(response);
            
            var identities = api.GetPerson(response.PersonId);

            identities.Should().Contain(Identifier(nhsNumber));
            
            
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.Testing.Utils;
using CHC.Consent.Tests.Api.Controllers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;


namespace CHC.Consent.Tests.Integration.Api
{
    using Random = Testing.Utils.Random;

    public class ApiClientTests : WebIntegrationTest
    {
        private static readonly object sync = new object();

        /// <inheritdoc />
        public ApiClientTests(ITestOutputHelper output, WebServerFixture fixture) : base(fixture, output)
        {
            using (var scope = Server.Host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ConsentContext>();
                if (context.Set<AuthorityEntity>().Any(_ => _.SystemName == "medway")) return;
                lock (sync)
                {
                    if (context.Set<AuthorityEntity>().Any(_ => _.SystemName == "medway")) return;
                    output.WriteLine($"Creating Medway Authority");
                    context.Add(new AuthorityEntity("Medway", 150, "medway"));
                    context.SaveChanges();
                }
            }
        }

        [Theory, MemberData(nameof(IdentityTestData))]
        public void CanSendIdentitiesToServer(IIdentifierValueDto identifier, Action<IIdentifierValueDto> checkResult)
        {
            PersonCreatedResult response=null;
            ApiClient.Invoking(
                    _ => response  = _.PutPerson(
                        new PersonSpecification(
                            new List<IIdentifierValueDto> {identifier},
                            "medway",
                            new List<MatchSpecification>
                            {
                                new MatchSpecification {Identifiers = new List<IIdentifierValueDto> {identifier}}
                            }))
                )
                .Should().NotThrow();
                
            Assert.NotNull(response);
            
            var identities = ApiClient.GetPerson(response.PersonId);

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
                        .Which.Value.Should()
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
                yield return test ?? IsEquivalentTo(value);
            }

            return tests.Select(ToEnumerable).Select(Enumerable.ToArray);
        }

        private static Action<IIdentifierValueDto> IsEquivalentTo(IIdentifierValueDto value)
        {
            return i =>
                i.Should().BeOfType(value.GetType())
                    .And.Should().BeEquivalentTo(
                        value,
                        options => options.RespectingRuntimeTypes());
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
            var nhsNumber = NhsNumber("4334443434");
            var (name, _, _) = Name(Random.String(), Random.String());
            
            
            var response = ApiClient.PutPerson(
                new PersonSpecification(
                    new List<IIdentifierValueDto> {nhsNumber, name},
                    "medway",
                    new List<MatchSpecification>
                    {
                        new MatchSpecification {Identifiers = new List<IIdentifierValueDto> {nhsNumber}}
                    }));

            Assert.NotNull(response);
            
            var identities = ApiClient.GetPerson(response.PersonId);

            identities.Should().Contain(Identifier(nhsNumber));
            
            
        }
    }
}
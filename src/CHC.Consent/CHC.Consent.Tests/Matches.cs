using System;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Testing.Utils;
using FluentAssertions;

namespace CHC.Consent.Tests
{
    static class Matches
    {
        public static Action<IIdentifierValueDto> NhsNumber(string expectedValue) =>
            v => v.Should().BeEquivalentTo(Identifiers.Definitions.NhsNumber.Value(expectedValue));

        public static Action<IIdentifierValueDto> HospitalNumber(string expectedValue) =>
            v => v.Should().BeEquivalentTo(Identifiers.Definitions.HospitalNumber.Value( expectedValue));

        public static Action<IIdentifierValueDto> DateOfBirth(DateTime dateofBirth) =>
            v => v.Should().BeEquivalentTo(Identifiers.Definitions.DateOfBirth.Value( dateofBirth.Date));

        public static Action<IIdentifierValueDto> Name(string firstName, string lastName) => 
            v => v.Should().BeEquivalentTo(Identifiers.Definitions.Name.Value( new []
            {
                Identifiers.Definitions.FirstName.Value(firstName),
                Identifiers.Definitions.LastName.Value(lastName)
            }));

        public static Action<IIdentifierValueDto> Address(string line1 = null, string line2 = null, string line3 = null, string line4 = null, string line5 = null, string postcode = null) => _ => AssertAddress(_, line1, line2, line3, line4, line5, postcode);

        private static void AssertAddress(
            IIdentifierValueDto address,
            string line1 = null,
            string line2 = null,
            string line3 = null,
            string line4 = null,
            string line5 = null,
            string postcode = null)
        {
            address.Should().BeEquivalentTo(
                ClientIdentifierValues.Address(
                    line1,
                    line2,
                    line3,
                    line4,
                    line5,
                    postcode)
            );
        }
    }
}
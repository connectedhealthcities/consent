using System;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Testing.Utils
{
    public static class Identifiers
    {
        public static class Definitions
        {
            public static IdentifierDefinition String(string name) =>
                new IdentifierDefinition(name, new StringIdentifierType());

            public static IdentifierDefinition Date(string name) => 
                new IdentifierDefinition(name, new DateIdentifierType());
            
            public static IdentifierDefinition Composite(string name, params IdentifierDefinition[] fields) =>
                new IdentifierDefinition(name,
                    new CompositeIdentifierType(fields));
            
            public static IdentifierDefinition Enum(string name, params string[] values) =>
                new IdentifierDefinition(name, new EnumIdentifierType(values));

            public static readonly IdentifierDefinition DateOfBirth = Date("Date of Birth");

            public static IdentifierDefinition NhsNumber { get; } = String("NHS Number");

            public static IdentifierDefinition HospitalNumber { get; } = String("Bradford Hospital Number");

            public static readonly IdentifierDefinition AddressLine1 = String("Line 1");

            public static readonly IdentifierDefinition AddressLine2 = String("Line 2");

            public static readonly IdentifierDefinition AddressLine3 = String("Line 3");

            public static readonly IdentifierDefinition AddressLine4 = String("Line 4");

            public static readonly IdentifierDefinition AddressLine5 = String("Line 5");

            public static readonly IdentifierDefinition AddressPostcode = String("Postcode");

            public static IdentifierDefinition Address { get; }

            public static IdentifierDefinition Name { get; }
            public static readonly IdentifierDefinition Sex = Enum("Sex", "Male", "Female");

            public static readonly IdentifierDefinition FirstName = String("Given");
            public static readonly IdentifierDefinition LastName = String("Family");

            static Definitions()
            {
                Address = Composite(
                    "Address",
                    AddressLine1,
                    AddressLine2,
                    AddressLine3,
                    AddressLine4,
                    AddressLine5,
                    AddressPostcode
                );
                Name = Composite("Name", FirstName, LastName);
            }
        }

        public static PersonIdentifier PersonIdentifier<T>(T value, IdentifierDefinition definition) =>
            new PersonIdentifier(new SimpleIdentifierValue(value), definition);

        private static PersonIdentifier CompositeIdentifier(
            IdentifierDefinition definition, params PersonIdentifier[] identifiers) =>
            new PersonIdentifier(
                new CompositeIdentifierValue(
                    identifiers.Where(_ => _.Value.Value != null).ToArray()),
                definition
            );

        public static PersonIdentifier NhsNumber(string value) =>
            PersonIdentifier(value, Definitions.NhsNumber);

        public static PersonIdentifier HospitalNumber(string value) =>
            PersonIdentifier(value, Definitions.HospitalNumber);

        public static PersonIdentifier DateOfBirth(int year, int month, int date) => PersonIdentifier(
            new DateTime(year, month, date),
            Definitions.DateOfBirth);

        public static PersonIdentifier Address(
            string line1 = null,
            string line2 = null,
            string line3 = null,
            string line4 = null,
            string line5 = null,
            string postcode = null) =>
            CompositeIdentifier(
                Definitions.Address,
                PersonIdentifier(line1, Definitions.AddressLine1),
                PersonIdentifier(line2, Definitions.AddressLine2),
                PersonIdentifier(line3, Definitions.AddressLine3),
                PersonIdentifier(line4, Definitions.AddressLine4),
                PersonIdentifier(line5, Definitions.AddressLine5),
                PersonIdentifier(postcode, Definitions.AddressPostcode)
            );

        public static IdentifierDefinitionRegistry Registry { get; } =
            new IdentifierDefinitionRegistry(
                Definitions.NhsNumber,
                Definitions.HospitalNumber,
                Definitions.DateOfBirth,
                Definitions.Address,
                Definitions.Name);

        public static IdentifierDefinitionRegistryProvider Provider { get; }
            = new IdentifierDefinitionRegistryProvider(Registry);

        public static PersonIdentifier Name(string given, string family) =>
            CompositeIdentifier(
                Definitions.Name,
                PersonIdentifier(given, Definitions.FirstName),
                PersonIdentifier(family, Definitions.LastName)
            );
    }
}
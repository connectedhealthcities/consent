using System;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Common.Identity
{
    public static class KnownIdentifierDefinitions 
    {
        public static readonly IdentifierDefinition NhsNumber = new IdentifierDefinition(name: "NHS Number", type: new StringIdentifierType());
        public static readonly IdentifierDefinition Sex = new IdentifierDefinition(name: "Sex", type: new EnumIdentifierType("Female", "Male"));
        public static readonly IdentifierDefinition DateOfBirth = new IdentifierDefinition(name: "Date Of Birth", type: new DateIdentifierType());

        public static readonly IdentifierDefinition BradfordHospitalNumber = new IdentifierDefinition(name: "Bradford Hospital Number", type: new StringIdentifierType());

        public static class AddressParts
        {
            public static readonly IdentifierDefinition Line1 = new IdentifierDefinition("Line 1", new StringIdentifierType());
            public static readonly IdentifierDefinition Line2 = new IdentifierDefinition("Line 2", new StringIdentifierType());
            public static readonly IdentifierDefinition Line3 = new IdentifierDefinition("Line 3", new StringIdentifierType());
            public static readonly IdentifierDefinition Line4 = new IdentifierDefinition("Line 4", new StringIdentifierType());
            public static readonly IdentifierDefinition Line5 = new IdentifierDefinition("Line 5", new StringIdentifierType());
            public static readonly IdentifierDefinition Postcode = new IdentifierDefinition("Postcode", new StringIdentifierType());
        }

        public static readonly IdentifierDefinition Address = new IdentifierDefinition(
            name: "Address",
            type: new CompositeIdentifierType(
                AddressParts.Line1,
                AddressParts.Line2,
                AddressParts.Line3,
                AddressParts.Line4,
                AddressParts.Line5,
                AddressParts.Postcode
            ));

        public static class BirthOrderParts
        {
            public static readonly IdentifierDefinition PregnancyNumber = new IdentifierDefinition(
                name: "Pregnancy Number",
                type: new IntegerIdentifierType());
            public static readonly IdentifierDefinition BirthOrder = new IdentifierDefinition(
                name: "Birth Order",
                type: new IntegerIdentifierType());
        }

        public static readonly IdentifierDefinition BirthOrder = new IdentifierDefinition(
            name: "Birth Order",
            type: new CompositeIdentifierType(
                BirthOrderParts.PregnancyNumber,
                BirthOrderParts.BirthOrder
            ));

        public static class NameParts
        {
            public static readonly IdentifierDefinition GivenName = new IdentifierDefinition(
                "Given",
                new StringIdentifierType());
            public static readonly IdentifierDefinition FamilyName = new IdentifierDefinition(
                "Family",
                new StringIdentifierType());
        }

        public static readonly IdentifierDefinition Name = new IdentifierDefinition(
            name: "Name",
            type: new CompositeIdentifierType(
                NameParts.GivenName,
                NameParts.FamilyName));
        
        public static readonly IdentifierDefinition ContactNumber = new IdentifierDefinition(
            name: "Contact Number",
            type: new CompositeIdentifierType(
                new IdentifierDefinition(name: "Type", new StringIdentifierType()),
                new IdentifierDefinition(name: "Number", new StringIdentifierType())
                )
            );

        public static readonly IdentifierDefinitionRegistry KnownIdentifiers = new IdentifierDefinitionRegistry
        {
            NhsNumber,
            Sex,
            DateOfBirth,
            BradfordHospitalNumber,
            Address,
            BirthOrder,
            Name,
            ContactNumber
        };
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CompositeDefinitionType = CHC.Consent.Common.Infrastructure.Definitions.Types.CompositeDefinitionType;
using DateDefinitionType = CHC.Consent.Common.Infrastructure.Definitions.Types.DateDefinitionType;
using EnumDefinitionType = CHC.Consent.Common.Infrastructure.Definitions.Types.EnumDefinitionType;
using EvidenceDefinition = CHC.Consent.Api.Client.Models.EvidenceDefinition;
using IdentifierDefinition = CHC.Consent.Common.Identity.Identifiers.IdentifierDefinition;
using IDefinition = CHC.Consent.Common.Infrastructure.Definitions.IDefinition;
using IDefinitionType = CHC.Consent.Common.Infrastructure.Definitions.IDefinitionType;
using IntegerDefinitionType = CHC.Consent.Common.Infrastructure.Definitions.Types.IntegerDefinitionType;
using StringDefinitionType = CHC.Consent.Common.Infrastructure.Definitions.Types.StringDefinitionType;

namespace CHC.Consent.Testing.Utils
{
    public static class Identifiers
    {
        public static class Definitions
        {
            public static IdentifierDefinition String(string name) =>
                new IdentifierDefinition(name, new StringDefinitionType());

            public static IdentifierDefinition Date(string name) =>
                new IdentifierDefinition(name, new DateDefinitionType());

            public static IdentifierDefinition Composite(string name, params IdentifierDefinition[] fields) =>
                new IdentifierDefinition(
                    name,
                    new CompositeDefinitionType(fields));

            public static IdentifierDefinition Enum(string name, params string[] values) =>
                new IdentifierDefinition(name, new EnumDefinitionType(values));

            public static IdentifierDefinition DateOfBirth => KnownIdentifierDefinitions.DateOfBirth;

            public static IdentifierDefinition NhsNumber => KnownIdentifierDefinitions.NhsNumber;

            public static IdentifierDefinition HospitalNumber => KnownIdentifierDefinitions.BradfordHospitalNumber;

            public static IdentifierDefinition AddressLine1 => KnownIdentifierDefinitions.AddressParts.Line1;
            public static IdentifierDefinition AddressLine2 => KnownIdentifierDefinitions.AddressParts.Line2;
            public static IdentifierDefinition AddressLine3 => KnownIdentifierDefinitions.AddressParts.Line3;
            public static IdentifierDefinition AddressLine4 => KnownIdentifierDefinitions.AddressParts.Line4;
            public static IdentifierDefinition AddressLine5 => KnownIdentifierDefinitions.AddressParts.Line5;

            public static readonly IdentifierDefinition AddressPostcode =
                KnownIdentifierDefinitions.AddressParts.Postcode;

            public static IdentifierDefinition Address => KnownIdentifierDefinitions.Address;

            public static IdentifierDefinition Name => KnownIdentifierDefinitions.Name;
            public static IdentifierDefinition Sex => KnownIdentifierDefinitions.Sex;

            public static IdentifierDefinition FirstName => KnownIdentifierDefinitions.NameParts.GivenName;
            public static IdentifierDefinition LastName => KnownIdentifierDefinitions.NameParts.FamilyName;

            public static IdentifierDefinition Integer(string name) =>
                new IdentifierDefinition(name, new IntegerDefinitionType());
        }

        public static PersonIdentifier PersonIdentifier<T>(T value, IdentifierDefinition definition) =>
            new PersonIdentifier(new SimpleIdentifierValue(value), definition);

        private static PersonIdentifier CompositeIdentifier(
            IdentifierDefinition definition, params PersonIdentifier[] identifiers) =>
            new PersonIdentifier(
                new CompositeIdentifierValue<PersonIdentifier>(
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
                Definitions.Name,
                Definitions.Sex);

        public static PersonIdentifier Name(string given, string family) =>
            CompositeIdentifier(
                Definitions.Name,
                PersonIdentifier(given, Definitions.FirstName),
                PersonIdentifier(family, Definitions.LastName)
            );

        public static IList<Api.Client.Models.IdentifierDefinition> ConvertToClientDefinitions(
            this IdentifierDefinitionRegistry registry) =>
            registry.Cast<IdentifierDefinition>().Select(ConvertToClientDefinition).ToList();

        public static List<Api.Client.Models.IdentifierDefinition> ConvertToClientDefinitions(
            this IEnumerable<IdentifierDefinition> definitions) =>
            definitions.Select(ConvertToClientDefinition).ToList();

        public static Api.Client.Models.IdentifierDefinition ConvertToClientDefinition(this IdentifierDefinition definition)
        {
            return new Api.Client.Models.IdentifierDefinition(
                definition.SystemName,
                ConvertToClientType<IdentifierDefinition>(definition.Type, ConvertToClientDefinition),
                definition.Name
            );
        }

        public static IList<EvidenceDefinition> ConvertToClientDefinitions(EvidenceDefinitionRegistry registry) =>
            registry.Cast<Common.Consent.Evidences.EvidenceDefinition>().Select(ConvertToClientDefinition).ToList();

        public static EvidenceDefinition ConvertToClientDefinition(
            this Common.Consent.Evidences.EvidenceDefinition definition)
        {
            return new EvidenceDefinition(
                definition.SystemName,
                ConvertToClientType<Common.Consent.Evidences.EvidenceDefinition>(
                    definition.Type,
                    ConvertToClientDefinition),
                definition.Name
            );
        }

        private static Api.Client.Models.IDefinitionType ConvertToClientType<TInternalDefinition>(
            IDefinitionType internalType, 
            Func<TInternalDefinition, Api.Client.Models.IDefinition> convertToClientDefinition) 
            where TInternalDefinition:IDefinition
        {
            switch (internalType)
            {
                case CompositeDefinitionType composite:
                    return new Api.Client.Models.CompositeDefinitionType(composite.SystemName,
                        composite.Identifiers.Cast<TInternalDefinition>().Select(convertToClientDefinition).ToList());
                case DateDefinitionType date:
                    return new Api.Client.Models.DateDefinitionType(date.SystemName);
                case EnumDefinitionType @enum:
                    return new Api.Client.Models.EnumDefinitionType(
                        systemName: @enum.SystemName,
                        values: @enum.Values.ToList());
                case IntegerDefinitionType integer:
                    return new Api.Client.Models.IntegerDefinitionType(integer.SystemName);
                case StringDefinitionType @string:
                    return new Api.Client.Models.StringDefinitionType(@string.SystemName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(internalType));
            }
        }
    }
}
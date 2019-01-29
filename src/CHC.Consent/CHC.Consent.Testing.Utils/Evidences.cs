using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Infrastructure;
using EnumDefinitionType = CHC.Consent.Common.Infrastructure.Definitions.Types.EnumDefinitionType;
using EvidenceDefinition = CHC.Consent.Common.Consent.Evidences.EvidenceDefinition;
using IIdentifierValueDto = CHC.Consent.Api.Client.Models.IIdentifierValueDto;
using ServerDto = CHC.Consent.Api.Infrastructure.IIdentifierValueDto;
using StringDefinitionType = CHC.Consent.Common.Infrastructure.Definitions.Types.StringDefinitionType;

namespace CHC.Consent.Testing.Utils
{
    public static class Evidences
    {
        public static EvidenceDefinition String(string name) => new EvidenceDefinition(name, new StringDefinitionType());
        public static EvidenceDefinition Enum(string name, params string[] values) => 
        new EvidenceDefinition(name, new EnumDefinitionType(values));
        
        public static Evidence MedwayEvidence(
            string competencyStatus = null, string givenBy = null, string takenBy = null)
        {
            var parts = new[]
            {
                new Evidence(KnownEvidence.MedwayParts.CompetentStatus, new SimpleIdentifierValue(competencyStatus)),
                new Evidence(KnownEvidence.MedwayParts.ConsentGivenBy, new SimpleIdentifierValue(givenBy)),
                new Evidence(KnownEvidence.MedwayParts.ConsentTakenBy, new SimpleIdentifierValue(takenBy)),
            };

            return new Evidence(
                KnownEvidence.Medway,
                new CompositeIdentifierValue<Evidence>(parts.Where(_ => _.Value.Value != null).ToArray()));
        }

        public static IIdentifierValueDto ClientDto(this EvidenceDefinition definition, string value)
            => new IdentifierValueDtoString(definition.SystemName, value);
        
        public static IIdentifierValueDto ClientDto(this EvidenceDefinition definition, long value)
                    => new IdentifierValueDtoInt64(definition.SystemName, value);
        
        public static IIdentifierValueDto ClientDto(this EvidenceDefinition definition, params IIdentifierValueDto[] values)
            => new IdentifierValueDtoIIdentifierValueDto(definition.SystemName, values);

        public static IIdentifierValueDto ClientDto(
            this EvidenceDefinition definition, IEnumerable<IIdentifierValueDto> values)
            => definition.ClientDto(values.ToArray());

        public static IIdentifierValueDto ClientMedwayDto(string status=null, string givenBy=null, string takenBy=null)
        {
            var dtos = new List<IIdentifierValueDto>();
            if(status != null) dtos.Add(KnownEvidence.MedwayParts.CompetentStatus.ClientDto(status));
            if(givenBy != null) dtos.Add(KnownEvidence.MedwayParts.ConsentGivenBy.ClientDto(givenBy));
            if(takenBy != null) dtos.Add(KnownEvidence.MedwayParts.ConsentTakenBy.ClientDto(takenBy));
            return KnownEvidence.Medway.ClientDto(dtos);
        }
        
        public static IIdentifierValueDto ClientImportFileDto(string baseUri=null, long? line=null, long? offset=null)
        {
            var dtos = new List<IIdentifierValueDto>();
            if(baseUri != null) dtos.Add(KnownEvidence.ImportFileParts.BaseUri.ClientDto(baseUri));
            if(line != null) dtos.Add(KnownEvidence.ImportFileParts.LineNumber.ClientDto((long)line));
            if(offset != null) dtos.Add(KnownEvidence.MedwayParts.ConsentTakenBy.ClientDto((long)offset));
            return KnownEvidence.ImportFile.ClientDto(dtos);
        }
        
        public static ServerDto ServerDto<T>(this EvidenceDefinition definition, T value)
            => new IdentifierValueDto<T>(definition.SystemName, value);
        
        public static ServerDto ServerMedwayDto(string status=null, string givenBy=null, string takenBy=null)
        {
            var dtos = new List<ServerDto>();
            if(status != null) dtos.Add(KnownEvidence.MedwayParts.CompetentStatus.ServerDto(status));
            if(givenBy != null) dtos.Add(KnownEvidence.MedwayParts.ConsentGivenBy.ServerDto(givenBy));
            if(takenBy != null) dtos.Add(KnownEvidence.MedwayParts.ConsentTakenBy.ServerDto(takenBy));
            return KnownEvidence.Medway.ServerDto(dtos.ToArray());
        }
        
        public static ServerDto ServerImportFileDto(string baseUri=null, long? line=null, long? offset=null)
        {
            var dtos = new List<ServerDto>();
            if(baseUri != null) dtos.Add(KnownEvidence.ImportFileParts.BaseUri.ServerDto(baseUri));
            if(line != null) dtos.Add(KnownEvidence.ImportFileParts.LineNumber.ServerDto((long)line));
            if(offset != null) dtos.Add(KnownEvidence.MedwayParts.ConsentTakenBy.ServerDto((long)offset));
            return KnownEvidence.ImportFile.ServerDto(dtos.ToArray());
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.Core;
using CHC.Consent.Identity.Core;
using CHC.Consent.Identity.SimpleIdentity;
using CHC.Consent.Import.Core;

namespace CHC.Consent.Common.Import
{
    public class ImportFileReader : IEnumerable<PersonSpecification>
    {
        private readonly IStandardDataDatasource source;
        private readonly IIdentityKindStore identityKinds;
        private readonly IEvidenceKindStore evidenceKinds;

        public ImportFileReader(
            IStandardDataDatasource source, 
            IIdentityKindStore identityKinds,
            IEvidenceKindStore evidenceKinds)
        {
            this.source = source;
            this.identityKinds = identityKinds;
            this.evidenceKinds = evidenceKinds;
        }

        public IEnumerable<PersonSpecification> People()
        {
            return source.People.Select(Convert);
        }
        
        public PersonSpecification Convert(IImportRecord record)
        {
            var identities = record.Identities.Select(Convert).ToList();
            return new PersonSpecification
            {
                Identities = identities,
                MatchIdentity = record.MatchIdentity.Select(_ => Convert(_, identities)).ToList(),
                MatchSubjectIdentity = record.MatchStudyIdentity.Select(_ => GetIdentityFor(_, identities)).ToList(),
                Evidence = Convert(record.Evidence)
            };
        }

        public List<IEvidence> Convert(IEnumerable<EvidenceRecord> evidenceRecords)
        {
            return evidenceRecords.Select(Convert).ToList();
        }

        /// <remarks>
        /// TODO: What if evidence kind is invalid/non-existant?
        /// </remarks>
        public IEvidence Convert(EvidenceRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record), "Cannot convert evidence from a null record");
            var evidenceKind = FindEvidenceKindByExternalId(record.EvidenceKindExternalId);
            if (evidenceKind == null)
            {
                throw new InvalidOperationException(
                    $"Cannot find evidence kind with external id {record.EvidenceKindExternalId}");
            }
            return new ReadEvidence
            {
                Evidence = record.Evidence,
                EvidenceKindId = evidenceKind.Id
            };
        }

        public IEvidenceKind FindEvidenceKindByExternalId(string evidenceKindExternalId)
        {
            return evidenceKinds.FindEvidenceKindByExternalId(evidenceKindExternalId);
        }

        private class ReadEvidence : IEvidence
        {
            /// <inheritdoc />
            public Guid EvidenceKindId { get; set; }

            /// <inheritdoc />
            public string Evidence { get; set; }
        }

        private IMatch Convert(MatchRecord record, IEnumerable<IIdentity> identities)
        {
            if (record is MatchByIdentityKindIdRecord byIdKindId)
            {
                return Convert(byIdKindId, identities);
            }
            throw new NotImplementedException($"Cannot convert Match from {record.GetType()}");
        }

        /// <remarks>
        /// TODO: match identity by referenced ID from document
        /// TODO: what if match is an invalid external id
        /// TODO: what if the references are incorrect?
        /// </remarks>
        private IIdentityMatch Convert(MatchByIdentityKindIdRecord byIdKindId, IEnumerable<IIdentity> identities)
        {
            var identity = GetIdentityFor(byIdKindId, identities);
            return
                new IdentityMatch
                {
                    Match = identity
                };
        }

        private IIdentity GetIdentityFor(MatchByIdentityKindIdRecord byIdKindId, IEnumerable<IIdentity> identities)
        {
            var id = identityKinds.FindIdentityKindByExternalId(byIdKindId.IdentityKindExternalId).Id;

            var identity = identities.Single(_ => _.IdentityKindId == id);
            return identity;
        }

        private class IdentityMatch : IIdentityMatch
        {
            /// <inheritdoc />
            public IIdentity Match { get; set; }
        }

        public IIdentity Convert(IdentityRecord record)
        {
            //TODO: what if external id is invalid?
            var identityKindId = identityKinds.FindIdentityKindByExternalId(record.IdentityKindExternalId).Id;

            if(record is SimpleIdentityRecord)
            {
                return new SimpleIdentityKindProvider().ConvertToIdentity(identityKindId, record);
            }
            
            //TODO: report error rather than throwing error?
            throw new InvalidOperationException($"Cannot convert IdentityRecord of type {record.GetType()}");
        }
        
        public IEnumerator<PersonSpecification> GetEnumerator()
        {
            return People().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
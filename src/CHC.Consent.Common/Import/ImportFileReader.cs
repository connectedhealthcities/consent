using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.Common.Import
{
    public class ImportFileReader : IEnumerable<PersonSpecification>
    {
        private readonly IStandardDataDatasource source;
        private readonly IIdentityKindStore identityKinds;

        public ImportFileReader(IStandardDataDatasource source, IIdentityKindStore identityKinds)
        {
            this.source = source;
            this.identityKinds = identityKinds;
        }

        public class SimpleIdentitySpecification : ISimpleIdentity
        {
            public SimpleIdentitySpecification(Guid identityKindId, string value)
            {
                Value = value;
                IdentityKindId = identityKindId;
            }

            public Guid IdentityKindId { get;  }
            public string Value { get;  }
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
                MatchSubjectIdentity = record.MatchStudyIdentity.Select(_ => GetIdentityFor(_, identities)).ToList()
            };
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

            if(record is SimpleIdentityRecord simple)
            {
                return new SimpleIdentitySpecification(identityKindId, simple.Value);
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
using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public class PersistedPerson : IPerson
    {
        public virtual Guid Id { get; set; }

        public virtual ICollection<PersistedIdentity> Identities { get; protected set; } =
            new List<PersistedIdentity>();

        IEnumerable<IIdentity> IPerson.Identities => Identities;

        public virtual ICollection<PersistedSubjectIdentifier> SubjectIdentifiers { get; protected set; } =
            new List<PersistedSubjectIdentifier>();

        IEnumerable<ISubjectIdentifier> IPerson.SubjectIdentifiers => SubjectIdentifiers;


        /// <summary>
        /// For persistence/rehydration
        /// </summary>
        protected PersistedPerson()
        {
        }

        public PersistedPerson(IEnumerable<PersistedIdentity> identities)
        {
            foreach (var identity in identities)
            {
                AddIdentity(identity);
            }
            
        }

        private void AddIdentity(PersistedIdentity identity)
        {
            identity.Person = this;
            Identities.Add(identity);
        }

        /// <remarks>TODO:Check for existing identifier for identifiers</remarks>
        public virtual ISubjectIdentifier AddSubjectIdentifier(
            IStudy study, string subjectIdentifier, IEnumerable<IIdentity> identifiers)
        {
            var persisted = new PersistedSubjectIdentifier(
                this,
                study.Id,
                subjectIdentifier,
                identifiers.Select(FindIdentity));
            SubjectIdentifiers.Add(persisted);
            return persisted;
        }

        private PersistedIdentity FindIdentity(IIdentity existingIdentity)
        {
            return Identities.FirstOrDefault(
                _ => _.IdentityKindId == existingIdentity.IdentityKindId
                     && _.HasSameValueAs(existingIdentity)
            );
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public class PersistedSubjectIdentifier : ISubjectIdentifier
    {
        public virtual long Id { get; protected set; }
        public virtual Guid StudyId { get; protected set; }
        public virtual string SubjectIdentifier { get; protected set; }
        
        public virtual PersistedPerson Person { get; protected set; }

        public virtual ICollection<PersistedIdentity> Identities { get; protected set; } =
            new List<PersistedIdentity>();

        IEnumerable<IIdentity> ISubjectIdentifier.Identities => Identities; 

        /// <remarks>for persistence</remarks>
        protected PersistedSubjectIdentifier()
        {
        }

        public PersistedSubjectIdentifier(
            PersistedPerson person, 
            Guid studyId, 
            string identifier, 
            IEnumerable<PersistedIdentity> identities
                )
        {
            Person = person;
            StudyId = studyId;
            SubjectIdentifier  = identifier;
            Identities = identities.ToArray();
        }
    }
}
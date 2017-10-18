using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.NHibernate.Identity
{
    public class SubjectIdentifier : Entity, ISubjectIdentifier
    {
        public virtual Guid StudyId { get; protected set; }
        public virtual string TheSubjectIdentifier { get; set; }

        string ISubjectIdentifier.SubjectIdentifier => TheSubjectIdentifier;

        public virtual Person Person { get; protected set; }

        public virtual ICollection<Identity> Identities { get; protected set; } =
            new List<Identity>();

        IEnumerable<IIdentity> ISubjectIdentifier.Identities => Identities; 

        /// <remarks>for persistence</remarks>
        protected SubjectIdentifier()
        {
        }

        public SubjectIdentifier(
            Person person, 
            Guid studyId, 
            string identifier, 
            IEnumerable<Identity> identities
                )
        {
            Person = person;
            StudyId = studyId;
            TheSubjectIdentifier  = identifier;
            Identities = identities.ToArray();
        }
    }
}
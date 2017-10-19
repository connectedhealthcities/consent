using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Identity
{
    public class Person : Entity, IPerson, INHibernateSecurable
    {
        public virtual ICollection<Identity> Identities { get; protected set; } =
            new List<Identity>();

        IEnumerable<IIdentity> IPerson.Identities => Identities;

        public virtual ICollection<SubjectIdentifier> SubjectIdentifiers { get; protected set; } =
            new List<SubjectIdentifier>();

        IEnumerable<ISubjectIdentifier> IPerson.SubjectIdentifiers => SubjectIdentifiers;
        
        /// <summary>
        /// For persistence/rehydration
        /// </summary>
        protected Person()
        {
            Acl = new AccessControlList(this);
        }

        public Person(IEnumerable<Identity> identities) : this()
        {
            foreach (var identity in identities)
            {
                AddIdentity(identity);
            }
        }

        private void AddIdentity(Identity identity)
        {
            identity.Person = this;
            Identities.Add(identity);
        }

        /// <remarks>TODO:Check for existing identifier for identifiers</remarks>
        public virtual ISubjectIdentifier AddSubjectIdentifier(
            IStudy study, string subjectIdentifier, IEnumerable<IIdentity> identifiers)
        {
            var persisted = new SubjectIdentifier(
                this,
                study.Id,
                subjectIdentifier,
                identifiers.Select(FindIdentity));
            SubjectIdentifiers.Add(persisted);
            return persisted;
        }

        private Identity FindIdentity(IIdentity existingIdentity)
        {
            return Identities.FirstOrDefault(
                _ => _.IdentityKindId == existingIdentity.IdentityKindId
                     && _.HasSameValueAs(existingIdentity)
            );
        }

        /// <inheritdoc />
        IAccessControlList ISecurable.AccessControlList => Acl;

        /// <inheritdoc />
        public virtual AccessControlList Acl { get; protected set; }
    }
}
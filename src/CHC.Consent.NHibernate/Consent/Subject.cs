using System.Collections.Generic;
using CHC.Consent.Common.Core;
using CHC.Consent.Core;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Consent
{
    public class Subject : Entity, ISubject, INHibernateSecurable
    {
        protected Subject()
        {
            Acl = new AccessControlList(this);
        }

        /// <inheritdoc />
        public Subject(Study study, string identifier)
        {
            Study = study;
            Identifier = identifier;
            Acl = new AccessControlList(this, study.Acl);
        }


        public virtual Study Study { get; set; }
        /// <inheritdoc />
        IStudy ISubject.Study => Study;

        /// <inheritdoc />
        public virtual string Identifier { get; set; }

        public virtual ICollection<Consent> Consents { get; protected set; } = new List<Consent>();

        /// <inheritdoc />
        IEnumerable<IConsent> ISubject.Consents => Consents;

        /// <inheritdoc />
        IAccessControlList ISecurable.AccessControlList => Acl;

        /// <inheritdoc />
        public virtual AccessControlList Acl { get; }
    }
}
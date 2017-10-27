using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.Security;
using NHibernate;

namespace CHC.Consent.NHibernate.Consent
{
    public class Study : Entity, IStudy, INHibernateSecurable
    {
        public Study()
        {
            Acl = new AccessControlList(this);
        }

        /// <inheritdoc />
        IAccessControlList ISecurable.AccessControlList => Acl;

        /// <inheritdoc />
        public virtual AccessControlList Acl { get; protected set; }

        public virtual ISet<Subject> Subjects { get; protected set; } = new HashSet<Subject>();        
    }
}
using System;
using CHC.Consent.Common.Core;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Consent
{
    public class Study : Entity, IStudy, INHibernateSecurable
    {
        /// <inheritdoc />
        IAccessControlList ISecurable.AccessControlList => Acl;

        /// <inheritdoc />
        public virtual AccessControlList Acl { get; protected set; } = new AccessControlList();
    }
}
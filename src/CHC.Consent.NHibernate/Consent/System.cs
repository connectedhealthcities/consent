using CHC.Consent.Core;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Consent
{
    public class System : Entity, ISystem, INHibernateSecurable
    {
        /// <inheritdoc />
        public System()
        {
            Acl = new AccessControlList(this);
        }

        /// <inheritdoc />
        IAccessControlList ISecurable.AccessControlList => Acl;
        
        /// <inheritdoc />
        public virtual AccessControlList Acl { get; protected set; }
    }
}
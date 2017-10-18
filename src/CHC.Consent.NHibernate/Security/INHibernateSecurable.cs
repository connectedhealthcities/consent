using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public interface INHibernateSecurable : ISecurable
    {
        AccessControlList Acl { get; }
    }
}
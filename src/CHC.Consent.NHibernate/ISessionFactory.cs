using NHibernate;

namespace CHC.Consent.NHibernate
{
    public interface ISessionFactory
    {
        ISession StartSession();
    }
}
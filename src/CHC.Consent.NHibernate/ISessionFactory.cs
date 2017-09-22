using NHibernate;
using NHibernate.Dialect.Function;

namespace CHC.Consent.NHibernate
{
    public interface ISessionFactory
    {
        ISession StartSession();
    }
}
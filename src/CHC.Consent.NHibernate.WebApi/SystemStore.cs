using System;
using System.Linq;
using CHC.Consent.Core;
using CHC.Consent.WebApi.Abstractions.Consent;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.WebApi
{
    public class SystemStore: ISystemStore
    {
        public Func<ISession> GetSession { get; }

        public SystemStore(Func<ISession> getSession)
        {
            GetSession = getSession;
        }

        /// <inheritdoc />
        public ISystem GetSystem()
        {
            return GetSession().Query<Consent.System>().SingleOrDefault();
        }
    }
}
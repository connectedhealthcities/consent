using System;
using System.Linq;
using CHC.Consent.Identity.Core;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.Identity
{
    public class IdentityKindStore : IIdentityKindStore
    {
        private readonly Func<ISession> sessionAccessor;

        public IdentityKindStore(Func<ISession> sessionAccessor)
        {
            this.sessionAccessor = sessionAccessor;
        }

        public IIdentityKind FindIdentityKindByExternalId(string externalId)
        {
            return sessionAccessor().Query<IdentityKind>()
                .SetOptions(o => { o.SetCacheable(true); })
                .FirstOrDefault(_ => _.ExternalId == externalId);
        }

        public IIdentityKind AddIdentityKind(string externalId) 
        {
            var identityKind = new IdentityKind {ExternalId = externalId};
            sessionAccessor().Save(identityKind);
            return identityKind;
        }
    }
}
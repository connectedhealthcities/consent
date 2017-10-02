using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Identity.Core;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.Identity
{
    public class IdentityKindStore : IIdentityKindStore
    {
        private readonly ISessionFactory sessionFactory;

        public IdentityKindStore(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public IIdentityKind FindIdentityKindByExternalId(string externalId)
        {
            using (var session = sessionFactory.StartSession())
            {
                return session.Query<IdentityKind>()
                    .SetOptions(o => { o.SetCacheable(true); })
                    .FirstOrDefault(_ => _.ExternalId == externalId);
            }
        }

        public IIdentityKind AddIdentity(string externalId) =>
            sessionFactory.AsTransaction(
                s =>
                {
                    var identityKind = new IdentityKind {ExternalId = externalId};
                    s.Save(identityKind);
                    return identityKind;
                });
        
    }
}
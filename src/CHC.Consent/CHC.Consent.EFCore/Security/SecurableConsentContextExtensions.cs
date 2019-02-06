using System.Linq;
using NeinLinq;

namespace CHC.Consent.EFCore.Security
{
    public static class SecurableConsentContextExtensions
    {
        public static void GrantPermission<T>(
            this ConsentContext context,
            T securable,
            ConsentRole role,
            string permission) where T : class, ISecurable
        {
            if (context.Set<T>().ToInjectable()
                .Any(_ => _ == securable && _.GrantsPermissionToRole(role, permission))) return;
            
            var permissionEntity = context.Set<PermissionEntity>().SingleOrDefault(_ => _.Access == permission);
            var rolePrincipal = context.Set<RoleSecurityPrincipal>().SingleOrDefault(_ => _.Role == role)
                                ?? new RoleSecurityPrincipal{ Role = role };
            
            context.Set<AccessControlEntity>().Add(
                new AccessControlEntity
                {
                    ACL = securable.ACL,
                    Permission = permissionEntity,
                    Prinicipal = rolePrincipal
                });
        }
    }
}
using System;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common.Infrastructure;
using NeinLinq;

namespace CHC.Consent.EFCore.Security
{
    public static class SecurableQueryExtensions
    {
        [InjectLambda]
        public static bool GrantsPermission<T>(this T securable, IUserProvider user, params string[] permissions) 
            where T:ISecurable
        {
            throw new NotImplementedException();
        }

        public static Expression<Func<T, IUserProvider, string[], bool>> GrantsPermission<T>() where T : ISecurable =>
            (securable, user, permissions) => securable.ACL.Entries.Any(
                acl => permissions.Contains(acl.Permission.Access) && (
                           ((UserSecurityPrincipal) acl.Prinicipal).User.UserName == user.UserName
                           || user.Roles.Contains(((RoleSecurityPrincipal) acl.Prinicipal).Role.Name)));


        public static IQueryable<T> WithReadPermissionGrantedTo<T>(this IQueryable<T> securables, IUserProvider user)
            where T : ISecurable
            => securables.Where(s => s.GrantsPermission(user, "read"));

    }
}
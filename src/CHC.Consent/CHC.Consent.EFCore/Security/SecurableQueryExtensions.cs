using System;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common.Infrastructure;
using NeinLinq;

namespace CHC.Consent.EFCore.Security
{
    public static class SecurableQueryExtensions
    {
        private static class CompiledLambdas<T> where T:ISecurable
        {
            static CompiledLambdas()
            {
                GrantsPermissionToUser = GrantsPermissionToUser<T>().Compile();
                GrantsPermissionToRole = GrantsPermissionToRole<T>().Compile();
            }

            public static Func<T, IUserProvider, string[], bool> GrantsPermissionToUser { get; }
            public static Func<T, ConsentRole, string[], bool> GrantsPermissionToRole { get; }
        }


        [InjectLambda]
        public static bool GrantsPermissionToRole<T>(this T securable, ConsentRole role, params string[] permissions)
            where T : ISecurable
            => CompiledLambdas<T>.GrantsPermissionToRole(securable, role, permissions);

        public static Expression<Func<T, ConsentRole, string[], bool>> GrantsPermissionToRole<T>() where T : ISecurable
            =>
                (securable, role, permissions) =>
                    securable.ACL.Entries.Any(acl =>
                        permissions.Contains(acl.Permission.Access) && 
                        ((RoleSecurityPrincipal)acl.Prinicipal).Role == role);
        

        [InjectLambda]
        public static bool GrantsPermissionToUser<T>(this T securable, IUserProvider user, params string[] permissions)
            where T : ISecurable 
            => 
                CompiledLambdas<T>.GrantsPermissionToUser(securable, user, permissions);

        public static Expression<Func<T, IUserProvider, string[], bool>> GrantsPermissionToUser<T>() where T : ISecurable
            =>
                (securable, user, permissions) => securable.ACL.Entries.Any(
                    acl => permissions.Contains(acl.Permission.Access) && (
                               ((UserSecurityPrincipal) acl.Prinicipal).User.UserName == user.UserName
                               || user.Roles.Contains(((RoleSecurityPrincipal) acl.Prinicipal).Role.Name)));


        public static IQueryable<T> WithReadPermissionGrantedTo<T>(this IQueryable<T> securables, IUserProvider user)
            where T : ISecurable
            =>
                securables.Where(s => s.GrantsPermissionToUser(user, PermissionNames.Read));
    }
}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Security;
using IdentityModel;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CHC.Consent.Api.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUserProvider user;
        public ConsentContext Context { get; }

        /// <inheritdoc />
        public IndexModel(ConsentContext context, IUserProvider user)
        {
            this.user = user;
            Context = context;
        }


        public string SubjectId { get; private set; }
        public IEnumerable<StudyEntity> Studies { get; private set; }


        public void OnGet()
        {
            var roles = user.Roles.ToArray();
            var userName = user.UserName;
            Studies = Context.Studies.Where(
                    _ => _.ACL.Entries.Any(
                        acl => acl.Permission.Access == "Read" && (
                                   ((UserSecurityPrincipal) acl.Prinicipal).User.UserName == userName
                                   || roles.Contains(((RoleSecurityPrincipal) acl.Prinicipal).Role.Name))))
                .ToImmutableArray();

            SubjectId = User.GetSubjectId();
            
        }
    }
}
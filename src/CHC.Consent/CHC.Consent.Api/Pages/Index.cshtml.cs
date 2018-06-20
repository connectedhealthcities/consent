using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Security;
using IdentityModel;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CHC.Consent.Api.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private IConsentRepository Consent { get; }
        private readonly IUserProvider user;

        /// <inheritdoc />
        public IndexModel(IConsentRepository consent, IUserProvider user)
        {
            Consent = consent;
            this.user = user;
        }

        public IEnumerable<Study> Studies { get; private set; }

        public IActionResult OnGet()
        {
            Studies = Consent.GetStudies(user);

            if (Studies.Count() == 1)
            {
                return RedirectToPage("Studies", new {id = Studies.Single().Id.Id});
            }

            return Page();
        }
    }
}
using System.Net.Http;
using System.Threading.Tasks;
using CHC.Consent.Web.UI.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CHC.Consent.Web.UI.Pages
{
    public class SetupModel : PageModel
    {
        public HomeController.ApiClient ApiClient { get; }

        public enum Step { Start, Authenticated, UnAuthenticated }

        public Step CurrentStep { get; private set; } = Step.Start;

        /// <inheritdoc />
        public SetupModel(HomeController.ApiClient apiClient)
        {
            ApiClient = apiClient;
        }


        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnGetStartSetupAsync()
        {
            if(!User.Identity.IsAuthenticated) return Challenge();

            var client = await ApiClient.GetClientAsync();
            var response = await client.PostAsync("bootstrap/", new StringContent(""));
            response.EnsureSuccessStatusCode();

            return RedirectToPage("Home");
        }
    }
}
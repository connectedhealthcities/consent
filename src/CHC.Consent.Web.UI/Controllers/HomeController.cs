using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CHC.Consent.Web.UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace CHC.Consent.Web.UI.Controllers
{
    [Authorize]
    public partial class HomeController : Controller
    {
        private IAuthenticationService authenticationService;
        private readonly ApiClient apiClient;

        public HomeController(IAuthenticationService authenticationService, ApiClient apiClient)
        {
            this.authenticationService = authenticationService;
            this.apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
           
            ViewBag.Response = await apiClient.GetStringAsync("person");
            var tokens = new Dictionary<string, string>();
            if (User.Identity.IsAuthenticated)
            {
                var authenicationResult = await authenticationService.AuthenticateAsync(HttpContext, null);

                tokens = authenicationResult.Properties.GetTokens()
                    .ToDictionary(_ => _.Name, _ => _.Value);
            }
            ViewBag.Tokens = tokens;
            
            
            
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
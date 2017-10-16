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

namespace CHC.Consent.Web.UI.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IAuthenticationService authenticationService;

        public HomeController(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }
        
        public async Task<IActionResult> Index()
        {
            var client = new HttpClient {BaseAddress = new Uri("http://localhost:49410/v0.1-dev/")};
            client.DefaultRequestHeaders.Authorization= new AuthenticationHeaderValue("bearer", await HttpContext.GetTokenAsync("id_token"));
            ViewBag.Response = await client.GetStringAsync("person");
            Dictionary<string, string> tokens = new Dictionary<string, string>();
            if (User.Identity.IsAuthenticated)
            {
                var authenicationResult = await authenticationService.AuthenticateAsync(HttpContext, null);

                tokens = authenicationResult.Properties.GetTokens()
                    .ToDictionary(_ => _.Name, _ => _.Value);

                string accessToken = await HttpContext.GetTokenAsync("access_token");
                string idToken = await HttpContext.GetTokenAsync("id_token");
                
                // Now you can use them. For more info on when and how to use the 
                // access_token and id_token, see https://auth0.com/docs/tokens
            }
            ViewBag.Tokens = tokens;
            
            
            
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Authorize(Policy = "Test")]
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
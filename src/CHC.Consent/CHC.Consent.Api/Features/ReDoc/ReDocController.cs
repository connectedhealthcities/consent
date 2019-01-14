using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.ReDoc
{
    [Route("/redoc")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ReDocController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}
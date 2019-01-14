using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Infrastructure.Web
{
    public class NotImplementedResult : ActionResult, IActionResult
    {
        /// <inheritdoc />
        public override void ExecuteResult(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.NotImplemented;
        }
    }
}
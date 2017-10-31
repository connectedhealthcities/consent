using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CHC.Consent.WebApi.Infrastructure
{
    public class ConflictResult : StatusCodeResult
    {
        public string Action { get; }
        public string ControllerName { get; }
        public RouteValueDictionary RouteValues { get; set; }
        
        public ConflictResult([AspMvcAction]string action=null, [AspMvcController]string controllerName=null, object routeValues=null) : base(409)
        {
            Action = action;
            ControllerName = controllerName;
            RouteValues = new RouteValueDictionary(routeValues);
        }

        public IUrlHelper UrlHelper { get; set; }

        /// <inheritdoc />
        public override void ExecuteResult(ActionContext context)
        {
            var request = context.HttpContext.Request;

            var location = (UrlHelper ?? context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(context)).Action(
                Action,
                ControllerName,
                RouteValues,
                request.Scheme,
                request.Host.ToUriComponent());

            if (!string.IsNullOrEmpty(location))
            {
                context.HttpContext.Response.Headers["Location"] = location;
            }

            base.ExecuteResult(context);
        }
    }
}
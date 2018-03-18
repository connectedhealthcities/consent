using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CHC.Consent.Api.Infrastructure.Web
{
    public class SeeOtherOjectActionResult : ObjectResult
    {
        /// <inheritdoc />
        public SeeOtherOjectActionResult(string action, string controller = null, object routeValues = null, object result=null )
            : base(result)
        {
            this.action = action;
            this.controller = controller;
            this.routeValues = routeValues;
            StatusCode = (int) HttpStatusCode.SeeOther;
        }

        private readonly string action;
        private readonly string controller;
        private readonly object routeValues;
        
        public IUrlHelper UrlHelper { get; set; }

        /// <inheritdoc />
        public override void OnFormatting(ActionContext context)
        {
            var resquest = context.HttpContext.Request;
            var response = context.HttpContext.Response;
            var responseHeaders = response.GetTypedHeaders();
            var services = context.HttpContext.RequestServices;

            var urlHelper = UrlHelper
                            ?? services.GetService<IUrlHelperFactory>()
                                .GetUrlHelper(context);
            var uriString = 
                urlHelper
                    .Action(action, controller, routeValues, resquest.Scheme, resquest.Host.ToUriComponent());

            if(string.IsNullOrEmpty(uriString))
                throw new InvalidOperationException("No Routes Matched");
            
            responseHeaders.Location = new Uri(uriString);
            
            base.OnFormatting(context);
        }

    }
}
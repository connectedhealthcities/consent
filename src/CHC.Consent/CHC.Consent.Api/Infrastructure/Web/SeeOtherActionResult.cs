using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CHC.Consent.Api.Infrastructure.Web
{
    public class SeeOtherActionResult : ActionResult
    {
        /// <inheritdoc />
        public SeeOtherActionResult(string action, string controller) : this(action, controller, null)
        {
            
        }

        /// <inheritdoc />
        public SeeOtherActionResult(string action, object values) : this(action, null, values)
        {
            this.action = action;
            this.values = values;
        }

        /// <inheritdoc />
        public SeeOtherActionResult(string action, string controller = null, object values = null) 
        {
            this.action = action;
            this.controller = controller;
            this.values = values;
        }

        private string action;
        private string controller;
        private object values;
        
        public IUrlHelper UrlHelper { get; set; }

        /// <inheritdoc />
        public override void ExecuteResult(ActionContext context)
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
                .Action(action, controller, values, resquest.Scheme, resquest.Host.ToUriComponent());

            if(string.IsNullOrEmpty(uriString))
                throw new InvalidOperationException("No Routes Matched");
            
            response.StatusCode = (int)HttpStatusCode.SeeOther;
            responseHeaders.Location = new Uri(uriString);
        }
    }
}
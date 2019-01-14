using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;

[assembly: AspMvcViewLocationFormat("~/Features/{3}/{1}/{0}.cshtml")]
[assembly: AspMvcViewLocationFormat("~/Features/IdentityServer/{1}/{0}.cshtml")]
[assembly: AspMvcViewLocationFormat("~/Features/{1}/{0}.cshtml")]
[assembly: AspMvcViewLocationFormat("~/Features/{3}/Shared/{0}.cshtml")]
[assembly: AspMvcViewLocationFormat("~/Features/Shared/{0}.cshtml")]


namespace CHC.Consent.Api.Infrastructure.Web
{
    /// <summary>
    /// works with <see cref="FeatureConvention"/> to provide feature folders
    /// </summary>
    public class FeatureViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues([NotNull] ViewLocationExpanderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            switch (context.ActionContext.ActionDescriptor)
            {
                case ControllerActionDescriptor controllerActionDescriptor:
                    var featureName = controllerActionDescriptor.Properties["feature"] as string;
                    context.Values["feature"] = featureName;
                break;
            }
        }

        public IEnumerable<string> ExpandViewLocations(
            [NotNull] ViewLocationExpanderContext context,
            [NotNull] IEnumerable<string> viewLocations)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (viewLocations == null) throw new ArgumentNullException(nameof(viewLocations));
            if (!context.Values.TryGetValue("feature", out var featureName) || string.IsNullOrEmpty(featureName))
            {
                return viewLocations;
            }
            
            return viewLocations.Select(location => location.Replace("{3}", featureName));
        }
    }
}
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CHC.Consent.Api.Infrastructure.Web
{
    public class FeatureConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            controller.Properties.Add("feature", 
                GetFeatureName(controller.ControllerType));
        }
        private static string GetFeatureName(Type controllerType)
        {
            var tokens = controllerType.FullName.Split('.');
            if (tokens.All(t => t != "Features")) return "";
            var featureName = tokens
                .SkipWhile(t => !t.Equals("features",
                    StringComparison.CurrentCultureIgnoreCase))
                .Skip(1)
                .Take(1)
                .FirstOrDefault();
            return featureName;
        }
    }
}
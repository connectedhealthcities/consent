using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace CHC.Consent.Api.Infrastructure.Web
{
    public static class FeatureViewMvcBuilderExtensions
    {
        public static IMvcBuilder AddFeatureFolders(this IMvcBuilder builder)
        {
            return builder
                .AddMvcOptions(options => options.Conventions.Add(new FeatureConvention()))
                .AddRazorOptions(
                    options =>
                    {
                        // {0} - Action Name
                        // {1} - Controller Name
                        // {2} - Area Name
                        // {3} - Feature Name
                        // Replace normal view location entirely
                        options.ViewLocationFormats.Clear();
                        options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
                        options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
                        options.ViewLocationFormats.Add("/Features/{3}/{1}/{0}.cshtml");
                        options.ViewLocationFormats.Add("/Features/{3}/{0}.cshtml");
                        options.ViewLocationFormats.Add("/Features/{3}/Shared/{0}.cshtml");
                        options.ViewLocationFormats.Add("/Features/Shared/{0}.cshtml");
                        options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
                    });
        }
    }
}
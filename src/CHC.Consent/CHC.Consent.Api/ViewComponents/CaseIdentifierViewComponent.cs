using System.Threading.Tasks;
using CHC.Consent.Common.Consent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace CHC.Consent.Api.ViewComponents
{
    [ViewComponent(Name = "CaseIdentifier")]
    public class CaseIdentifierViewComponent : ViewComponent
    {
        private class EmptyViewComponentResult : IViewComponentResult
        {
            private EmptyViewComponentResult()
            {
            }

            public void Execute(ViewComponentContext context)
            {
            }

            public Task ExecuteAsync(ViewComponentContext context) => Task.CompletedTask;

            public static IViewComponentResult Instance { get; } = new EmptyViewComponentResult();
        }

        private static IViewComponentResult Empty => EmptyViewComponentResult.Instance;
        
        public Task<IViewComponentResult> InvokeAsync(CaseIdentifier identifier)
        {
            return Task.FromResult(identifier == null ? Empty : View(identifier.GetType().Name, identifier));
        }

    }
}

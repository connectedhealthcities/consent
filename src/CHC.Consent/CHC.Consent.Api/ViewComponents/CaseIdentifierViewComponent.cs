using System.Threading.Tasks;
using CHC.Consent.Common.Consent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace CHC.Consent.Api.ViewComponents
{
    [ViewComponent(Name = "CaseIdentifier")]
    public class CaseIdentifierViewComponent : ViewComponent
    {
        private class EmptyViewComonentResult : IViewComponentResult
        {
            private EmptyViewComonentResult()
            {
            }

            public void Execute(ViewComponentContext context)
            {
            }

            public Task ExecuteAsync(ViewComponentContext context) => Task.CompletedTask;

            public static IViewComponentResult Instance { get; } = new EmptyViewComonentResult();
        }

        private IViewComponentResult Empty => EmptyViewComonentResult.Instance;


        public async Task<IViewComponentResult> InvokeAsync(CaseIdentifier identifier)
        {
            if (identifier == null) return Empty;

            return View(identifier.GetType().Name, identifier);
        }

    }
}

using System.Threading.Tasks;
using CHC.Consent.Common.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace CHC.Consent.Api.ViewComponents
{
    [ViewComponent(Name = "Identifier")]
    public class IdentifierViewComponent : ViewComponent
    {
        private class EmptyViewComponentResult : IViewComponentResult
        {
            private EmptyViewComponentResult(){}

            public void Execute(ViewComponentContext context){}

            public Task ExecuteAsync(ViewComponentContext context) => Task.CompletedTask;

            public static IViewComponentResult Instance { get; } = new EmptyViewComponentResult();
        }
        
        private static IViewComponentResult Empty() => EmptyViewComponentResult.Instance;

        public Task<IViewComponentResult> InvokeAsync(IPersonIdentifier identifier)
        {
            return Task.FromResult(identifier == null ? Empty() : View(identifier.GetType().Name, identifier));
        }
    }
}
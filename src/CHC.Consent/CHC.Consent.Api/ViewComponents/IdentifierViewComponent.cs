using System.Threading.Tasks;
using CHC.Consent.Common.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace CHC.Consent.Api.ViewComponents
{
    [ViewComponent(Name = "Identifier")]
    public class IdentifierViewComponent : ViewComponent
    {
        private class EmptyViewComonentResult : IViewComponentResult
        {
            private EmptyViewComonentResult(){}

            public void Execute(ViewComponentContext context){}

            public Task ExecuteAsync(ViewComponentContext context) => Task.CompletedTask;

            public static IViewComponentResult Instance { get; } = new EmptyViewComonentResult();
        }
        
        private IViewComponentResult Empty() => EmptyViewComonentResult.Instance;

        public async Task<IViewComponentResult> InvokeAsync(IPersonIdentifier identifier)
        {
            if (identifier == null) return Empty();
            
            return View(identifier.GetType().Name, identifier);
        }
        
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.Common.Infrastructure.Definitions.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace CHC.Consent.Api.ViewComponents
{
    [ViewComponent(Name = "Identifier")]
    public class IdentifierViewComponent : ViewComponent
    {
        private IdentifierTypeToView ViewNames { get; }

        private class EmptyViewComponentResult : IViewComponentResult
        {
            private EmptyViewComponentResult(){}

            public void Execute(ViewComponentContext context){}

            public Task ExecuteAsync(ViewComponentContext context) => Task.CompletedTask;

            public static IViewComponentResult Instance { get; } = new EmptyViewComponentResult();
        }
        
        private static IViewComponentResult Empty() => EmptyViewComponentResult.Instance;

        /// <inheritdoc />
        public IdentifierViewComponent(IdentifierDefinitionRegistry identifiers)
        {
            ViewNames = identifiers.Accept(new IdentifierTypeToView());
        }
        
        private class IdentifierTypeToView : IDefinitionVisitor
        {
            private readonly Dictionary<string, string> DefinitionsToViewNames = new Dictionary<string, string>();

            private void Add(IDefinition definition, string viewName)
            {
                DefinitionsToViewNames[definition.SystemName] = viewName;
            }

            public string this[IDefinition definition] => DefinitionsToViewNames[definition.SystemName];

            /// <inheritdoc />
            public void Visit(IDefinition definition, DateDefinitionType type)
            {
                Add(definition, "Date");
            }

            /// <inheritdoc />
            public void Visit(IDefinition definition, EnumDefinitionType type)
            {
                Add(definition, "enum");
            }

            /// <inheritdoc />
            public void Visit(IDefinition definition, CompositeDefinitionType type)
            {
                Add(definition, "composite");
            }

            /// <inheritdoc />
            public void Visit(IDefinition definition, IntegerDefinitionType type)
            {
                Add(definition, "integer");
            }

            /// <inheritdoc />
            public void Visit(IDefinition definition, StringDefinitionType type)
            {
                Add(definition, "string");
            }
        }

        public IViewComponentResult Invoke(PersonIdentifier identifier, DefinitionRegistry registry, bool raw)
        {
            if (identifier == null) return Empty();
            var result = View(GetViewName(registry, identifier.Definition), identifier.Value);
            result.ViewData["Raw"] = raw;
            return result;
        }

        private string GetViewName(DefinitionRegistry registry, IDefinition definition)
        {
            var definitionSystemName = definition.SystemName;
            if (ViewEngine.FindView(ViewContext, definitionSystemName, false).Success)
                return definitionSystemName;
            return GetDefaultViewName(registry, definition);
        }

        private string GetDefaultViewName(DefinitionRegistry registry, IDefinition definition)
        {
            return (registry == null ? ViewNames : registry.Accept(new IdentifierTypeToView()))[definition];
        }
    }
}
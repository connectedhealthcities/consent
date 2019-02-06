using System.Collections.Generic;
using System.Threading.Tasks;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.Common.Infrastructure.Definitions.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace CHC.Consent.Api.ViewComponents
{
    [ViewComponent(Name = "Identifier")]
    public class IdentifierViewComponent : ViewComponent
    {
        private IdentifierTypeToView viewNames;

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
            viewNames = identifiers.Accept(new IdentifierTypeToView());
        }
        
        private class IdentifierTypeToView : IDefinitionVisitor
        {
            private readonly Dictionary<string, string> DefinitionsToViewNames = new Dictionary<string, string>();

            private void Add(IDefinition definition, string viewName)
            {
                DefinitionsToViewNames[definition.SystemName] = viewName;
            }

            public string this[IDefinition definition] => GetViewName(definition);

            public string GetViewName(IDefinition definition) => DefinitionsToViewNames[definition.SystemName];
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

        public Task<IViewComponentResult> InvokeAsync(PersonIdentifier identifier, IdentifierDefinitionRegistry registry)
        {
            if (identifier == null) return Task.FromResult(Empty());
            var v = (registry == null ? viewNames : registry.Accept(new IdentifierTypeToView()))[identifier.Definition];
            return Task.FromResult<IViewComponentResult>(View(v, identifier.Value));
        }
    }
}
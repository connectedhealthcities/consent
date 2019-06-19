using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Client.Models;

namespace CHC.Consent.DataTool.Features.ExportData
{
    public static class IdentifierDefinitionEnumerableExtensions
    {
        public static IdentifierDefinition GetDefinition(
            this IEnumerable<IdentifierDefinition> definitions, string systemName)
        {
            try
            {
                return definitions.Single(d => d.SystemName == systemName);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException($"Cannot find field called '{systemName}' ", e);
            }
        }
    }
}
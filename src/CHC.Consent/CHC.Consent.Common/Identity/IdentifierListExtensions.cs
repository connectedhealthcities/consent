using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHC.Consent.Common.Identity
{
    public static class IdentifierListExtensions 
    {
        public static void EnsureHasNoInvalidDuplicates(this IEnumerable<Identifier> identifiers)
        {
            var invalidDuplicates =
                identifiers
                    .Where(identifer => !identifer.IdentifierType.CanHaveMultipleValues)
                    .GroupBy(identifer => identifer.IdentifierType)
                    .Where(identifiersByType => identifiersByType.Count() > 1)
                    .ToArray();

            if (!invalidDuplicates.Any()) return;
            
            var errors = new StringBuilder();
            errors.AppendLine("Invalid Duplicate identifiers found:");
            foreach (var duplicate in invalidDuplicates)
            {
                errors.AppendFormat("\t{0} has values: ", duplicate.Key.ExternalId);
                foreach (var value in duplicate.Select(i => i.Value))
                {
                    errors.AppendFormat("{0}, ", value);    
                }
                    
                errors.AppendLine();
            }
            throw new InvalidOperationException(errors.ToString());
        }
    }
}
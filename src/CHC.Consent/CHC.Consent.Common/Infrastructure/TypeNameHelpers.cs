using System;
using System.Reflection;

namespace CHC.Consent.Common.Infrastructure
{
    public static class TypeNameHelpers
    {
        public static TAttribute GetIdentiferAttribute<TAttribute>(Type identifierType)  
            where TAttribute: Attribute, ITypeName
        {
            var identifierAttribute = identifierType.GetCustomAttribute<TAttribute>();
            if (identifierAttribute == null)
            {
                throw new ArgumentException(
                    $"Cannot get attributes for {identifierType} as it has no {typeof(TAttribute).Name}");
            }

            return identifierAttribute;
        }
        
    }
}
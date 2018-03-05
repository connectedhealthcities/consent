using System;

namespace CHC.Consent.Common.Infrastructure
{
    public static class TypeHelpers
    {
        public static bool IsSubtypeOf(this Type type, Type baseType)
        {
            return baseType.IsAssignableFrom(type);
        }

        public static bool IsSubtypeOf<TBase>(this Type type)
        {
            return type.IsSubtypeOf(typeof(TBase));
        }
    }
}
using System;

namespace CHC.Consent.Utils
{
    public static class TypeExtension
    {
        public static bool IsInheritedFrom<T>(this Type @this) => @this.IsInheritedFrom(typeof(T));

        public static bool IsInheritedFrom(this Type @this, Type baseType)
        {
            return baseType.IsAssignableFrom(@this);
        }

        public static bool IsSubclassOf<T>(this Type type) => type.IsSubclassOf(typeof(T));
    }
}
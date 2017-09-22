using System;

namespace CHC.Consent.Common.Utils
{
    public static class UnitHelpers
    {
        public static Func<T, Unit> AsUnitFunc<T>(this Action<T> action)
        {
            return t =>
            {
                action(t);
                return Unit.Value;
            };
        }
    }
}
using System;

namespace CHC.Consent.Utils
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

        public static Func<Unit> AsUnitFunc(this Action action)
        {
            return () =>
            {
                action();
                return Unit.Value;
            }; 
        }
    }

    public static class FuncHelpers
    {
        public static Func<TArgs, T> IgnoreParams<TArgs, T>(this Func<T> getValue) => _ => getValue();

        public static Action<T> Then<T>(this Action<T> @this, Action<T> next)
        {
            return t =>
            {
                @this(t);
                next(t);
            };
        }
    }
}
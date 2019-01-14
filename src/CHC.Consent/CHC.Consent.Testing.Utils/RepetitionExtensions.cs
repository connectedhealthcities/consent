using System;
using System.Collections.Generic;
using System.Linq;

namespace CHC.Consent.Testing.Utils
{
    public static class RepetitionExtensions
    {
        public static void Times(this int repetitions, Action @do)
        {
            if (repetitions < 0) throw new ArgumentOutOfRangeException(nameof(repetitions), "repetitions must be >= 0");
            for (var i = 0; i < repetitions; i++)
            {
                @do();
            }
        }

        public static void Times<T>(this int repetitions, Func<T> @do)
        {
            void DoAction() => @do();
            repetitions.Times(DoAction);
        }

        public static IEnumerable<T> Of<T>(this int repetitions, Func<T> make)
        {
            T Make(int _) => make();
            return Enumerable.Range(0, repetitions).Select(Make);
        }
    }
}
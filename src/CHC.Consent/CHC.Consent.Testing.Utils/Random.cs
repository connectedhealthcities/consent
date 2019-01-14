using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

namespace CHC.Consent.Testing.Utils
{
    public static class Random
    {
        private static readonly System.Random random = new System.Random();

        public static byte[] RandomBytes(this int length)
        {
            var buffer = new byte[length];
            random.NextBytes(buffer);
            return buffer;
        }
        public static string String(int lengthish=32)
        {
            return Convert.ToBase64String(RandomBytes(lengthish)).TrimEnd('=');
        }

        public static DateTime Date()
        {
            while (true)
            {
                var value = Long();
                if(value <= DateTime.MinValue.Ticks) continue;
                if(value >= DateTime.MaxValue.Ticks) continue;
                
                var date = DateTime.FromBinary(value);
                if (SqlDateTime.MinValue.Value <= date && date <= SqlDateTime.MaxValue.Value)
                {
                    return date;
                }
            }
        }

        public static long Long()
        {
            return BitConverter.ToInt64(RandomBytes(sizeof(long)), 0);
        }
        
        public static T Enum<T>() where T:struct
        {
            return RandomItem(System.Enum.GetValues(typeof(T)).Cast<T>().ToArray());
        }

        private static T RandomItem<T>(IReadOnlyCollection<T> values)
        {
            return values.ElementAt(random.Next(0, values.Count));
        }

        public static T Item<T>(params T[] items)
        {
            return items[random.Next(0, items.Length)];
        }
        
    }
}
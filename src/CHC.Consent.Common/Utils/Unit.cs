﻿namespace CHC.Consent.Common.Utils
{
    /// <summary>
    /// An classy alternative to Void 
    /// </summary>
    public class Unit
    {
        public static Unit Value { get; } = new Unit();

        private Unit()
        {
        }
    }
}
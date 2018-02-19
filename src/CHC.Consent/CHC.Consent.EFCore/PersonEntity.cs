using System;
using System.Runtime.InteropServices.ComTypes;
using AutoMapper;
using CHC.Consent.Common;

namespace CHC.Consent.EFCore
{
    public class PersonEntity : Person
    {
        public override ushort? BirthOrder
        {
            get => (ushort?) BirthOrderValue;
            set => BirthOrderValue = value;
        }

        internal int? BirthOrderValue { get; set; }
    }

    
}
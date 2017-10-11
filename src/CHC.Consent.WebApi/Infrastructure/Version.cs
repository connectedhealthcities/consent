using System;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.WebApi.Infrastructure
{
    public static class Version
    {
        public const string V_0_1_Dev = "0.1-dev"; 
        
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited=false)]
        public class _0_1_DevAttribute : ApiVersionAttribute
        {
            /// <inheritdoc />
            public _0_1_DevAttribute() : base(V_0_1_Dev)
            {
            }
        }
    }
}
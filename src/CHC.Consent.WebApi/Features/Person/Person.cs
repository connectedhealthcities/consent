using System;
using CHC.Consent.WebApi.Abstractions;

namespace CHC.Consent.WebApi.Features.Person
{
    namespace ResponseModels
    {
        /// <summary>
        /// 
        /// </summary>
        public class Person
        {
            public Guid Id { get; set; }
        }
    }

    namespace RequestModels
    {
        public class GetPeople : IPagingProperties
        {
            public int Page { get; set; } = 0;
            public int PageSize { get; set; } = 500;
        }
    }
    
}
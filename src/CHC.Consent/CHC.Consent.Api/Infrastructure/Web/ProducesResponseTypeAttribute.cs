using System;
using System.Net;

namespace CHC.Consent.Api.Infrastructure.Web
{
    public class ProducesResponseTypeAttribute : Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute
    {
        /// <inheritdoc />
        public ProducesResponseTypeAttribute(int statusCode) : base(statusCode)
        {
        }

        /// <inheritdoc />
        public ProducesResponseTypeAttribute(Type type, int statusCode) : base(type, statusCode)
        {
        }
        
        public ProducesResponseTypeAttribute(Type type, HttpStatusCode statusCode) : base(type, (int)statusCode){}
        public ProducesResponseTypeAttribute(HttpStatusCode statusCode, Type type) : base(type, (int)statusCode){}
        public ProducesResponseTypeAttribute(HttpStatusCode statusCode) : base((int)statusCode){}
    }
}
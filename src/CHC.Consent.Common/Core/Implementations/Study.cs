using System;

namespace CHC.Consent.Common.Core
{
    public class Study : IStudy
    {
        public virtual Guid Id { get; protected set; }
        
    }
}
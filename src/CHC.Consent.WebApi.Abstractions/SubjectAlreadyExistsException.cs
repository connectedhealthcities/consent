using System;
using CHC.Consent.Common.Core;

namespace CHC.Consent.WebApi.Abstractions
{
    public class SubjectAlreadyExistsException : Exception
    {
        public IStudy Study { get; }
        public string Identifier { get; }

        public SubjectAlreadyExistsException(IStudy study, string identifier) : base($"{study} already has a subject with identifier {identifier}")
        {
            Study = study;
            Identifier = identifier;
        }
    }
}
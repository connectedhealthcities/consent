using System.Collections.Generic;
using CHC.Consent.Common.Core;

namespace CHC.Consent.Core
{
    public interface ISubject
    {
        IStudy Study { get; }
        string Identifier { get; }
        IEnumerable<IConsent> Consents { get; }
    }
}
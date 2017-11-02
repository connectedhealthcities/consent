using System.Collections.Generic;
using CHC.Consent.Common.Core;
using CHC.Consent.Security;

namespace CHC.Consent.Core
{
    public interface ISubject: ISecurable
    {
        IStudy Study { get; }
        string Identifier { get; }
        IEnumerable<IConsent> Consents { get; }
    }
}
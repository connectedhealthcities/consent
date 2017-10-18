using System.Collections.Generic;

namespace CHC.Consent.Security
{
    public interface IAccessControlList
    {
        IEnumerable<IAccessControlEntry> Permissions { get; }
    }
}
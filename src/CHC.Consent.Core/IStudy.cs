using System;
using CHC.Consent.Security;

namespace CHC.Consent.Common.Core
{
    public interface IStudy : ISecurable
    {
        Guid Id { get; }
    }
}
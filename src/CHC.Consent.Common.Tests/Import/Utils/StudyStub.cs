using System;
using CHC.Consent.Common.Core;

namespace CHC.Consent.Common.Tests.Import.Utils
{
    internal class StudyStub : IStudy { public Guid Id { get; } = Guid.NewGuid(); }
}
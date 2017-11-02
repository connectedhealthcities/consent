using System;
using CHC.Consent.Common.Core;
using CHC.Consent.Security;

namespace CHC.Consent.Common.Tests.Import.Utils
{
    internal class StudyStub : IStudy { public Guid Id { get; } = Guid.NewGuid();

        /// <inheritdoc />
        public IAccessControlList AccessControlList => throw new NotImplementedException();
    }
}
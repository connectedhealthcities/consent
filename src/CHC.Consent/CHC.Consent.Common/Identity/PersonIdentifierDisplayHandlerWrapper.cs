using System;
using JetBrains.Annotations;

namespace CHC.Consent.Common.Identity
{
    public class PersonIdentifierDisplayHandlerWrapper<TIdentifier> : IPersonIdentifierDisplayHandler
        where TIdentifier : IPersonIdentifier
    {
        private IPersonIdentifierDisplayHandler<TIdentifier> Handler { get; }

        /// <inheritdoc />
        public PersonIdentifierDisplayHandlerWrapper([NotNull] IPersonIdentifierDisplayHandler<TIdentifier> handler)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        /// <inheritdoc />
        public string DisplayName => Handler.DisplayName;
    }
}
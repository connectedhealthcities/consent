using System;

namespace CHC.Consent.Testing.Utils
{
    internal class NoopDisposable : IDisposable
    {
        public static IDisposable Instance { get; } = new NoopDisposable();
        public void Dispose() { }
    }
}
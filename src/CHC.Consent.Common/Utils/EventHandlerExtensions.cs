using System;

namespace CHC.Consent.Common.Utils
{
    public static class EventHandlerExtensions
    {
        public static void Trigger<T>(this EventHandler<T> handler, object sender, T args)
        {
            handler?.Invoke(sender, args);
        }
    }
}
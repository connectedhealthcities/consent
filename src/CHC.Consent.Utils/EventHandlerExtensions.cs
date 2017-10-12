using System;

namespace CHC.Consent.Utils
{
    public static class EventHandlerExtensions
    {
        public static void Trigger<T>(this EventHandler<T> handler, object sender, T args)
        {
            handler?.Invoke(sender, args);
        }

        public static void Trigger(this EventHandler handler, object sender, EventArgs args=null)
        {
            handler?.Invoke(sender, args ?? EventArgs.Empty);
        }
    }
}
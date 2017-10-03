using System;
using Xunit;

namespace CHC.Consent.Utils.Tests
{
    public class EventHandlerExtensionsTests
    {
        [Fact]
        public void NullHandlerDoesNotThrowNullReferenceException()
        {
            EventHandler<int> p = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            p.Trigger(null, 23);
        }

        event EventHandler<MyEventArgs> P;

        class MyEventArgs : EventArgs
        {
            public MyEventArgs(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [Fact]
        public void NonNullHandlerIsCalled()
        {
            object sender = new object();
            var raisedEvent = Assert.Raises<MyEventArgs>(
                handler => P += handler,
                handler => P -= handler,
                () => P.Trigger(sender, new MyEventArgs(54)));

            Assert.Same(sender, raisedEvent.Sender);
            Assert.Equal(54, raisedEvent.Arguments.Value);
        }


    }
}
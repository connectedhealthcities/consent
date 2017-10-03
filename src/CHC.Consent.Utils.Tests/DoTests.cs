using System;
using Xunit;

namespace CHC.Consent.Utils.Tests
{
    public class DoTests
    {
        [Fact]
        public void DoNothingDoesNothing()
        {
            Do.Nothing();
            Do.Nothing(2);
            Do.Nothing(1,2);
        }
    }
}

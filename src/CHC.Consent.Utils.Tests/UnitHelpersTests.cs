using System;
using Xunit;

namespace CHC.Consent.Utils.Tests
{
    public class UnitHelpersTests
    {
        [Fact]
        public void WrapsActionInAFunc()
        {
            Action<int> p = i => { };

            Assert.IsType<Func<int, Unit>>(p.AsUnitFunc());
        }

        [Fact]
        public void WrappedFunctionReturnsUnit()
        {
            Action<int> p = i => { };

            Assert.Equal(Unit.Value, p.AsUnitFunc()(23));
        }

        [Fact]
        public void WrappedFunctionCallsInnerFunction()
        {
            int? parameterValue = null;
            Action<int> p = i => { parameterValue = i; };

            p.AsUnitFunc()(78);

            Assert.Equal(78, parameterValue);
        }
    }
}
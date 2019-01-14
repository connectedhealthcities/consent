using System;
using CHC.Consent.Common.Infrastructure;
using Xunit;

namespace CHC.Consent.Tests.Infrastructure
{
    public class TypeHelpersTests
    {
        private static readonly Type SubType = typeof(Sub);

        interface IInterface { }
        abstract class AbstractBase {} 
        class Base : AbstractBase {}
        class Sub : Base, IInterface { }

        [Theory]
        [InlineData(typeof(Base))]
        [InlineData(typeof(object))]
        [InlineData(typeof(AbstractBase))]
        [InlineData(typeof(IInterface))]
        public void CorrectlyIdentifiesSubTypes(Type baseType)
        {
            Assert.True(SubType.IsSubtypeOf(baseType));
        }

        [Fact]
        public void CorrectlyIdentifiesSubTypesUsingGenerics()
        {
            Assert.True(SubType.IsSubtypeOf<Base>());
            Assert.True(SubType.IsSubtypeOf<AbstractBase>());
            Assert.True(SubType.IsSubtypeOf<IInterface>());
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(ArrayTypeMismatchException))]
        public void CorrectlyIdentifiesWhenATypeIsNotASubType(Type otherType)
        {
            Assert.False(SubType.IsSubtypeOf(otherType));
            Assert.False(otherType.IsSubtypeOf(SubType));
        }
        
    }
}
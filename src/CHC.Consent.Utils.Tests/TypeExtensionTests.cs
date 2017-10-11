using System;
using Xunit;

namespace CHC.Consent.Utils.Tests
{
    public class TypeExtensionTests
    {
        private static readonly Type BaseType = typeof(Base);

        public abstract class Base { } 
        
        public class SubClass : Base { }
        
        public class Unassociated { }

        [Fact]
        public void SubclassIsInheritedFromBase()
        {
            Assert.True(typeof(SubClass).IsInheritedFrom<Base>());
        }

        [Fact]
        public void BaseIsInheritedFromBase()
        {
            Assert.True(BaseType.IsInheritedFrom<Base>());
        }

        [Fact]
        public void UnassociatedClassIsNotInheritedFromBase()
        {
            Assert.False(typeof(Unassociated).IsInheritedFrom<Base>());
        }

        [Fact]
        public void BaseIsNotSubclassOfBase()
        {
            Assert.False(BaseType.IsSubclassOf<Base>());
        }

        [Fact]
        public void SubclassIsSubclassOfBase()
        {
            Assert.True(typeof(SubClass).IsSubclassOf<Base>());
        }

        [Fact]
        public void UnassociatedIsNotSubclassOfBase()
        {
            Assert.False(typeof(Unassociated).IsSubclassOf<Base>());
        }
        
    }
}
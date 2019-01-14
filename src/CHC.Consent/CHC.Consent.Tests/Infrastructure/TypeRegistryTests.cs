using System;
using Castle.Core.Internal;
using CHC.Consent.Common.Infrastructure;
using FakeItEasy;
using Xunit;

namespace CHC.Consent.Tests.Infrastructure
{
    public class TypeRegistryTests
    {
        private readonly TypeRegistry<BaseType, BaseTypeAttribute> registry =
            new TypeRegistry<BaseType, BaseTypeAttribute>();

        /// <summary>
        /// This needs to be public for dynamic proxy generation to happen
        /// </summary>
        public abstract class BaseType {}
        
        /// <summary>
        /// This needs to be public for dynamic proxy generation to happen
        /// </summary>
        public class BaseTypeAttribute : Attribute, ITypeName {
            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public BaseTypeAttribute(string name)
            {
                Name = name;
            }
        }
        
        [BaseType(nameof(Type1))]
        class Type1 : BaseType { }
        
        [BaseType(nameof(Type2))]
        class Type2 : BaseType { }
        
        class UnnamedType : BaseType { }


        [Fact]
        public void CallingGenericAddCallsAddWithCorrectAttributes()
        {
            var fakeRegistry = A.Fake<TypeRegistry<BaseType, BaseTypeAttribute>>();

            A.CallTo(() => fakeRegistry.Add<Type1>()).CallsBaseMethod();
            
            fakeRegistry.Add<Type1>();

            A.CallTo(() => 
                    fakeRegistry.Add(typeof(Type1), A<BaseTypeAttribute>.That.Matches(a => a.Name == nameof(Type1))))
                .MustHaveHappened();
        }
        
        [Fact]
        public void CanRegisterSubTypesByPassingAttribute()
        {
            Assert.True(typeof(Type2).IsSubtypeOf<BaseType>());
            registry.Add(typeof(Type2), typeof(Type2).GetTypeAttribute<BaseTypeAttribute>());
        }

        [Fact]
        public void CanRegisterSubTypesWithGenericMethodCall()
        {
            Assert.True(typeof(Type1).IsSubtypeOf<BaseType>());
            registry.Add<Type1>();
        }

        [Fact]
        public void CanRegisterSubTypesByPassingName()
        {
            Assert.True(typeof(UnnamedType).IsSubtypeOf<BaseType>());
            registry.Add(typeof(UnnamedType), "test");
        }

        [Fact]
        public void CannotRegisterUnrelatedType()
        {
            Assert.ThrowsAny<Exception>(() => registry.Add(typeof(TypeRegistryTests), new BaseTypeAttribute("Blah")));
            Assert.ThrowsAny<Exception>(() => registry.Add(typeof(TypeRegistryTests), "Blah"));
        }

        [Fact]
        public void CanRetrieveTypeByName()
        {
            registry.Add<Type1>();
            registry.Add<Type2>();
            registry.Add(typeof(UnnamedType), "test");


            Assert.Equal(typeof(UnnamedType), registry["test"]);

            Assert.True(registry.TryGetType(nameof(Type1), out var foundType));
            Assert.Equal(typeof(Type1), foundType);
        }
        
        [Fact]
        public void CannotGetTypeofUknownName()
        {
            Assert.False(registry.TryGetType(nameof(TypeRegistryTests), out var _));
            Assert.ThrowsAny<Exception>(() => registry[nameof(TypeRegistryTests)]);
        }
        
        [Fact]
        public void CanGetNameByType()
        {
            registry.Add<Type1>();
            registry.Add<Type2>();
            registry.Add(typeof(UnnamedType), "test");


            Assert.Equal("test", registry[typeof(UnnamedType)]);

            Assert.True(registry.TryGetName(typeof(Type2), out var foundType));
            Assert.Equal(nameof(Type2), foundType);
        }

        [Fact]
        public void CannotGetNameofUknownType()
        {
            Assert.False(registry.TryGetName(typeof(TypeRegistryTests), out var _));
            Assert.ThrowsAny<Exception>(() => registry[typeof(TypeRegistryTests)]);
        }
        
        
        [Fact]
        public void EnumeratesKnownMappings()
        {
            Action<ClassMapping> IsMapping<T>(string name)
            {
                return _ =>
                {
                     Assert.Equal(name, _.Name);
                    Assert.Equal(typeof(T), _.Type);
                }; 
            }
            
            registry.Add<Type1>();
            registry.Add<Type2>();
            registry.Add(typeof(UnnamedType), "test");


            Assert.Collection(
                registry,
                IsMapping<Type1>(nameof(Type1)),
                IsMapping<Type2>(nameof(Type2)),
                IsMapping<UnnamedType>("test")
            );
        }
    }
}
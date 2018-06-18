
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CHC.Consent.Tests.Api.Infrastructure.IdentifierDisplay
{
    public class IdentifierDisplayOptionsTests
    {
        class TestConfig
        {
            public Type Test { get; set; }
        }

        [Fact]
        public void CanNotBindTypeFromStringOptions()
        {
            var thisType = typeof(IdentifierDisplayOptionsTests);

            Assert.Throws<NotSupportedException>(
                () => TypeDescriptor.GetConverter(typeof(Type)).ConvertFromString(thisType.FullName));
            
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()
                {
                    [nameof(TestConfig.Test)] = thisType.AssemblyQualifiedName 
                }
            ).Build();

            Assert.Equal(null,  configuration.GetValue<Type>("Test"));

            Assert.Equal(null, configuration.Get<TestConfig>().Test);
        }
        
    }
}
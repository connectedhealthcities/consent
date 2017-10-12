using CHC.Consent.WebApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CHC.Consent.WebApi.Tests
{
    public class ServiceCollectionExtensionsTest
    {
        private interface IService{ }

        private class Service:IService{}

        private class ServiceDecorator : IService
        {
            public IService Inner { get; }

            /// <inheritdoc />
            public ServiceDecorator(IService inner)
            {
                Inner = inner;
            }
        }

        [Fact]
        public void DecoratesServiceCorrectly()
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, Service>();

            services.Decorate<IService>(inner => new ServiceDecorator(inner));

            var service = services.BuildServiceProvider().GetService<IService>();

            Assert.IsType<ServiceDecorator>(service);
            Assert.IsType<Service>(((ServiceDecorator) service).Inner);
        }
    }
}
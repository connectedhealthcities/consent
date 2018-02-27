using System.Buffers;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CHC.Consent.Api.Infrastructure.Web
{
    /// <summary>
    /// Adds <see cref="IdentityModelBinderProvider{TModel}"/> to the <see cref="MvcOptions.ModelBinderProviders"/>
    /// before the <see cref="BodyModelBinderProvider"/> so that they take precedence over the details
    /// </summary>
    /// <typeparam name="TRegistry">The <see cref="ITypeRegistry"/> type (resolved via constructor injection)</typeparam>
    /// <typeparam name="TModel">The `class` for which to bind</typeparam>
    public class IdentityModelBinderProviderConfiguration<TRegistry, TModel> : IConfigureOptions<MvcOptions>
        where TRegistry:ITypeRegistry
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ArrayPool<char> charPool;
        private readonly ObjectPoolProvider objectPoolProvider;
        private readonly ITypeRegistry registry;
        private readonly IHttpRequestStreamReaderFactory readerFactory;

        /// <inheritdoc />
        public IdentityModelBinderProviderConfiguration(
            ILoggerFactory loggerFactory,
            ArrayPool<char> charPool,
            ObjectPoolProvider objectPoolProvider,
            IHttpRequestStreamReaderFactory readerFactory,
            TRegistry registry)
        {
            this.loggerFactory = loggerFactory;
            this.charPool = charPool;
            this.objectPoolProvider = objectPoolProvider;
            this.registry = registry;
            this.readerFactory = readerFactory;
        }

        /// <inheritdoc />
        public void Configure(MvcOptions options)
        {
            var identityModelBinderProvider = new IdentityModelBinderProvider<TModel>(
                charPool,
                objectPoolProvider,
                options.SuppressInputFormatterBuffering,
                readerFactory,
                loggerFactory,
                options,
                registry.CreateSerializerSettings()
            );

            var defaultBodyProvider = options.ModelBinderProviders.FirstOrDefault(_ => _ is BodyModelBinderProvider);
            if (defaultBodyProvider != null)
            {
                options.ModelBinderProviders.Insert(
                    options.ModelBinderProviders.IndexOf(defaultBodyProvider),
                    identityModelBinderProvider);
            }
            else
            {
                options.ModelBinderProviders.Add(identityModelBinderProvider);
            }
        }


        
    }
}
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Common.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CHC.Consent.Api.Infrastructure.Web
{
    /// <summary>
    /// Overrides the <see cref="JsonSerializerSettings"/> when binding to <typeparamref name="TModel" /> with <see cref="FromBodyAttribute"/>
    /// </summary>
    /// <typeparam name="TModel">The <c>class</c> to match against when performing binding</typeparam>
    /// <remarks>only handles <c>JSON</c> data</remarks>
    public class IdentityModelBinderProvider<TModel> : IModelBinderProvider
    {
        private static readonly Type ModelType = typeof(TModel);
        private readonly ArrayPool<char> charPool;
        private readonly ObjectPoolProvider objectPoolProvider;
        private readonly bool suppressInputFormatterBuffering;
        private readonly IHttpRequestStreamReaderFactory readerFactory;
        private readonly ILoggerFactory loggerFactory;
        private readonly MvcOptions mvcOptions;
        private readonly JsonSerializerSettings serializerSettings;
        private readonly ILogger<IModelBinderProvider> Logger;

        public IdentityModelBinderProvider(
            ArrayPool<char> charPool,
            ObjectPoolProvider objectPoolProvider,
            bool suppressInputFormatterBuffering,
            IHttpRequestStreamReaderFactory readerFactory,
            ILoggerFactory loggerFactory,
            MvcOptions mvcOptions, 
            JsonSerializerSettings serializerSettings)
        {
            this.charPool = charPool;
            this.objectPoolProvider = objectPoolProvider;
            this.suppressInputFormatterBuffering = suppressInputFormatterBuffering;
            this.readerFactory = readerFactory;
            this.loggerFactory = loggerFactory;
            this.mvcOptions = mvcOptions;
            this.serializerSettings = serializerSettings;
            Logger = this.loggerFactory.CreateLogger<IModelBinderProvider>();
        }

        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.BindingInfo.BindingSource != BindingSource.Body) return null;
            if (ShouldBindTo(context) 
                
                )
            {
                Logger.LogDebug(
                    "Providing binder for Body of type {modelType}",
                    ModelType);

                return new BodyModelBinder(
                    new IInputFormatter[]
                    {
                        new JsonInputFormatter(
                            NullLogger.Instance,
                            serializerSettings,
                            charPool,
                            objectPoolProvider,
                            suppressInputFormatterBuffering)
                    },
                    readerFactory,
                    loggerFactory,
                    mvcOptions);
            }

            return null;
        }

        private static bool ShouldBindTo(ModelBinderProviderContext context)
        {
            var targetType = context.Metadata.ModelType;
            return targetType == ModelType 
                   || (targetType.IsArray && targetType.GetElementType() == ModelType)
                || IsEnumerableOfModelType(targetType);
        }

        private static bool IsEnumerableOfModelType(Type type)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(type)) return false;
            return type.GetInterfaces().Where(_ => _.IsGenericType && _.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(_ => _.GetGenericArguments()[0]).FirstOrDefault()
                == ModelType;
        }
    }
}
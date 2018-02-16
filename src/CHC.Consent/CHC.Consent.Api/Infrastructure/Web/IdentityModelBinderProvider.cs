using System;
using System.Buffers;
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
        }

        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.BindingInfo.BindingSource != BindingSource.Body) return null;
            if (context.Metadata.ModelType != ModelType) return null;

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
    }
}
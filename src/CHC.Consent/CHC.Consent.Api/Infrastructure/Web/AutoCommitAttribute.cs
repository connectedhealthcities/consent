using System;
using CHC.Consent.EFCore;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CHC.Consent.Api.Infrastructure.Web
{
    public class AutoCommitAttribute : Attribute, IFilterFactory
    {
        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return new AutoCommitActionFilter(serviceProvider);
        }

        private class AutoCommitActionFilter : IActionFilter
        {
            private readonly ConsentContext consentContext;
            private IDbContextTransaction transaction;
            private ILogger<AutoCommitActionFilter> logger;

            public AutoCommitActionFilter(IServiceProvider serviceProvider)
            {
                consentContext = serviceProvider.GetRequiredService<ConsentContext>();
                logger = serviceProvider.GetRequiredService<ILogger<AutoCommitActionFilter>>();
            }

            /// <inheritdoc />
            public void OnActionExecuting(ActionExecutingContext context)
            {
                try
                {
                    transaction =
                        consentContext.Database.CurrentTransaction == null
                            ? consentContext.Database.BeginTransaction()
                            : null;
                }
                catch (InvalidOperationException e)
                {
                    logger.LogWarning(
                        e,
                        "Cannot create transaction for {1}.{0}",
                        context.Controller,
                        context.ActionDescriptor.DisplayName);
                }
                
            }

            /// <inheritdoc />
            public void OnActionExecuted(ActionExecutedContext context)
            {
                if (context.Exception == null || context.ExceptionHandled)
                {
                    consentContext.SaveChanges();
                    transaction?.Commit();
                }
                else
                {
                    transaction?.Rollback();
                }
            }
        }

        /// <inheritdoc />
        public bool IsReusable => false;
    }
}
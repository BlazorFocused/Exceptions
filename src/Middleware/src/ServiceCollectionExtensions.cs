// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using BlazorFocused.Exceptions.Middleware.ApplicationBuilder;
using BlazorFocused.Exceptions.Middleware.ExceptionBuilder;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorFocused.Exceptions.Middleware;

/// <summary>
///     Extensions used to leverage Exception Middleware Framework in dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="serviceCollection">Current collection of services registered for dependency injection</param>
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        ///     Add Exception Middleware Services to current collection of service descriptors
        /// </summary>
        /// <param name="modifyOptions">Override default middleware option properties for custom responses</param>
        /// <returns><see cref="IExceptionsMiddlewareBuilder" /> used for further middleware extension/customization</returns>
        public IExceptionsMiddlewareBuilder AddExceptionsMiddleware(
            Action<ExceptionsMiddlewareOptions>? modifyOptions = null)
        {
            serviceCollection.Configure<ExceptionsMiddlewareOptions>(options => modifyOptions?.Invoke(options));

            return new ExceptionsMiddlewareBuilder(serviceCollection);
        }

        /// <summary>
        ///     Add Exception Handler Middleware Services to current collection of service descriptors
        /// </summary>
        /// <param name="modifyOptions">Override default middleware option properties for custom responses</param>
        /// <returns><see cref="IExceptionsMiddlewareBuilder" /> used for further middleware extension/customization</returns>
        public IExceptionsMiddlewareBuilder AddExceptionsHandler(
            Action<ExceptionsMiddlewareOptions>? modifyOptions = null)
        {
            serviceCollection.Configure<ExceptionsMiddlewareOptions>(options => modifyOptions?.Invoke(options));

            serviceCollection.AddExceptionHandler<ApplicationExceptionHandler>();

            return new ExceptionsMiddlewareBuilder(serviceCollection);
        }
    }
}

// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using BlazorFocused.Exceptions.Middleware.ApplicationBuilder;
using BlazorFocused.Exceptions.Middleware.ExceptionBuilder;

namespace BlazorFocused.Exceptions.Middleware;

/// <summary>
/// Extensions used to leverage Exception Middleware Framework in dependency injection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Exception Middleware Services to current collection of service descriptors
    /// </summary>
    /// <param name="serviceCollection">Current collection of services registered for dependency injection</param>
    /// <returns><see cref="IExceptionsMiddlewareBuilder"/> used for further middleware extension/customization</returns>
    public static IExceptionsMiddlewareBuilder AddExceptionsMiddleware(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<ExceptionsMiddlewareOptions>();

        return new ExceptionsMiddlewareBuilder(serviceCollection);
    }

    /// <summary>
    /// Add Exception Middleware Services to current collection of service descriptors
    /// </summary>
    /// <param name="serviceCollection">Current collection of services registered for dependency injection</param>
    /// <param name="modifyOptions">Override default middleware option properties for custom responses</param>
    /// <returns><see cref="IExceptionsMiddlewareBuilder"/> used for further middleware extension/customization</returns>
    public static IExceptionsMiddlewareBuilder AddExceptionsMiddleware(
        this IServiceCollection serviceCollection,
        Action<ExceptionsMiddlewareOptions> modifyOptions)
    {
        serviceCollection.Configure<ExceptionsMiddlewareOptions>(options => modifyOptions(options));

        return new ExceptionsMiddlewareBuilder(serviceCollection);
    }

    /// <summary>
    /// Add Exception Middleware to application request/response pipeline
    /// </summary>
    /// <param name="builder">Current application request pipeline configuration to append exception middleware activation</param>
    /// <returns>Current application request pipeline configuration</returns>
    public static IApplicationBuilder UseExceptionsMiddleware(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ApplicationBuilderMiddleware>();
}

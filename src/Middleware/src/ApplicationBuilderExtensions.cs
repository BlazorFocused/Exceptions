// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using BlazorFocused.Exceptions.Middleware.ApplicationBuilder;
using Microsoft.AspNetCore.Builder;

namespace BlazorFocused.Exceptions.Middleware;

/// <summary>
///     Extensions used to activate Exception Middleware Framework in dependency injection
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <param name="applicationBuilder">
    ///     Current application request pipeline configuration to append exception middleware
    ///     activation
    /// </param>
    extension(IApplicationBuilder applicationBuilder)
    {
        /// <summary>
        ///     Add Exception Middleware to application request/response pipeline
        /// </summary>
        /// <param name="useExceptionHandler">Distinguish between use of middleware vs exception handler implementation</param>
        /// <returns>Current application request pipeline configuration</returns>
        /// <remarks>
        ///     Use of middleware provides correlation Ids and log scoping by default. Handler only runs when an
        ///     unexpected exception is thrown
        /// </remarks>
        public IApplicationBuilder UseExceptionsMiddleware(
            bool useExceptionHandler = true) =>
            useExceptionHandler
                ? applicationBuilder.UseExceptionHandler()
                : applicationBuilder.UseMiddleware<ApplicationBuilderMiddleware>();
    }
}

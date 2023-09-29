// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace BlazorFocused.Exceptions.Middleware.ExceptionBuilder;

/// <summary>
/// Allows configuration of returned exception to client based on HttpContext properties
/// </summary>
/// <typeparam name="T"></typeparam>
internal class ExceptionResponseResolver<T> : ExceptionResponseResolver where T : Exception
{
    private readonly Func<HttpContext, T, ExceptionsMiddlewareResponse> exceptionMapping;

    /// <summary>
    /// Configures the return of an exception based on HttpContext properties
    /// </summary>
    /// <param name="exceptionMapping">Configures <see cref="ExceptionsMiddlewareResponse"/> based on HttpContext properties</param>
    public ExceptionResponseResolver(Func<HttpContext, T, ExceptionsMiddlewareResponse> exceptionMapping)
    {
        this.exceptionMapping = exceptionMapping;
    }

    /// <summary>
    /// Resolves configured exception response based on current exception
    /// </summary>
    /// <param name="httpContext">Current <see cref="HttpContext" /> of request</param>
    /// <param name="exception">Current exception that occurred during request</param>
    /// <returns>Response of prepared exception configuration</returns>
    /// <exception cref="ArgumentException">Thrown if exception not configured properly</exception>
    public override ExceptionsMiddlewareResponse Resolve(HttpContext httpContext, Exception exception) =>
        exception is not T castedException
            ? throw new ArgumentException($"Exception of type {exception.GetType().FullName} does not have configured response")
            : exceptionMapping(httpContext, castedException);
}

/// <summary>
///
/// </summary>
internal abstract class ExceptionResponseResolver
{
    /// <summary>
    /// Allows configuration of returned exception to client based on HttpContext properties
    /// </summary>
    /// <param name="httpContext">Current <see cref="HttpContext" /> of request</param>
    /// <param name="exception">Current exception that occurred during request</param>
    /// <returns>Response of prepared exception configuration</returns>
    public abstract ExceptionsMiddlewareResponse Resolve(HttpContext httpContext, Exception exception);
}

// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Net;

namespace BlazorFocused.Exceptions.Middleware;

/// <summary>
/// Configures Exception Middleware framework for application business logic
/// </summary>
public interface IExceptionsMiddlewareBuilder
{
    /// <summary>
    /// Configures a response status code for a given exception
    /// </summary>
    /// <typeparam name="TException">Type of exception to apply the given properties</typeparam>
    /// <param name="httpStatusCode">Configures status code returned for give exception</param>
    /// <returns>Current Exception Middleware Configuration Builder</returns>
    IExceptionsMiddlewareBuilder AddException<TException>(HttpStatusCode httpStatusCode)
        where TException : Exception;

    /// <summary>
    /// Configures a response status code and default response message returned for a given exception
    /// </summary>
    /// <typeparam name="TException">Type of exception to apply the given properties</typeparam>
    /// <param name="httpStatusCode">Expected http status code returned for the given exception</param>
    /// <param name="clientErrorMessage">Message returned to client for a given exception</param>
    /// <returns>Current Exception Middleware Configuration Builder</returns>
    IExceptionsMiddlewareBuilder AddException<TException>(HttpStatusCode httpStatusCode, string clientErrorMessage)
        where TException : Exception;

    /// <summary>
    /// Establish a response status code and response message (<see cref="ExceptionsMiddlewareResponse"/>) returned for a given exception
    /// </summary>
    /// <typeparam name="TException">Type of exception to apply the given properties</typeparam>
    /// <param name="buildExceptionResponse">Configured exception response properties based on <see cref="HttpContext"/></param>
    /// <returns>Current Exception Middleware Configuration Builder</returns>
    IExceptionsMiddlewareBuilder AddException<TException>(Func<HttpContext, TException, ExceptionsMiddlewareResponse> buildExceptionResponse)
        where TException : Exception;
}

// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using System.Net;

namespace BlazorFocused.Exceptions.Middleware;

/// <summary>
/// Base exception used to extend configured exceptions
/// </summary>
public abstract class ExceptionsMiddlewareException : Exception
{
    /// <summary>
    /// Desired status code of a given exception
    /// </summary>
    public HttpStatusCode StatusCode { get; private set; }

    /// <summary>
    /// Message that should be displayed to client/user in case of given exception
    /// </summary>
    public string ClientErrorMessage { get; init; }

    /// <summary>
    /// Creates exception with desire output configurations
    /// </summary>
    /// <param name="httpStatusCode">Desired status code of a given exception</param>
    /// <param name="message">Message that should be logged in case of given exception</param>
    /// <remarks>Passed in "message" will also be output to client/user if "clientErrorMessage" not specified</remarks>
    protected ExceptionsMiddlewareException(HttpStatusCode httpStatusCode, string message) :
        base(message)
    {
        StatusCode = httpStatusCode;
    }

    /// <summary>
    /// Creates exception with desire output configurations
    /// </summary>
    /// <param name="httpStatusCode">Desired status code of a given exception</param>
    /// <param name="message">Message that should be logged in case of given exception</param>
    /// <param name="innerException">Inner exception that triggered exception response</param>
    /// <remarks>Passed in "message" will also be output to client/user if "clientErrorMessage" not specified</remarks>
    protected ExceptionsMiddlewareException(HttpStatusCode httpStatusCode, string message, Exception innerException) :
        base(message, innerException)
    {
        StatusCode = httpStatusCode;
    }
}

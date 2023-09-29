// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using System.Net;
using BlazorFocused.Exceptions.Middleware;

namespace MiddlewareSample.Api.Exceptions;

public class CustomClientException : ExceptionsMiddlewareException
{
    private const HttpStatusCode defaultStatusCode = HttpStatusCode.Conflict;

    public CustomClientException(string message) :
        base(defaultStatusCode, message)
    { }

    public CustomClientException(HttpStatusCode httpStatusCode, string message) :
        base(httpStatusCode, message)
    { }

    public CustomClientException(string message, Exception innerException) :
        base(defaultStatusCode, message, innerException)
    { }

    public CustomClientException(HttpStatusCode httpStatusCode, string message, Exception innerException) :
        base(httpStatusCode, message, innerException)
    { }
}

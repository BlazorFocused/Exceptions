// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using System.Net;

namespace BlazorFocused.Exceptions.Middleware.ExceptionBuilder;

internal class ExceptionsMiddlewareBuilderOptions
{
    public Type ExceptionType { get; set; }

    public HttpStatusCode? HttpStatusCode { get; set; }

    public string DefaultMessage { get; set; }

    public ExceptionResponseResolver ExceptionResponseResolver { get; set; }
}

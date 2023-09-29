// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using System.Net;

namespace BlazorFocused.Exceptions.Middleware;

/// <summary>
/// Represents current context of an exception being handled in Exceptions Middleware
/// </summary>
public class ExceptionsMiddlewareResponse
{
    /// <summary>
    /// Desired status code of a given exception
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; set; }

    /// <summary>
    /// Message that should be logged in case of given exception
    /// </summary>
    public string ErrorMessage { get; set; }
}

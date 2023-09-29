﻿// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using MiddlewareSample.Api.Exceptions;
using System.Net;
using BlazorFocused.Exceptions.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom exceptions and status codes
builder.Services
    // .AddExceptionsMiddleware() -> Use this extension for  default correlation key/value
    .AddExceptionsMiddleware(options =>
    {
        options.CorrelationKey = "X-TestCorrelation-Id";
        options.CorrelationKey = CORRELATION_HEADER_KEY;
        options.ConfigureCorrelationValue = (httpContext) => { return httpContext.TraceIdentifier; };
    }) // Use this extension for  default correlation key/value
        .AddException<RandomException>(HttpStatusCode.FailedDependency);

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Register Exceptions API Middleware
app.UseExceptionsMiddleware();

// Endpoints
app.MapGet("/ThrowRandomException", () =>
{
    throw new RandomException();
})
.WithOpenApi();

app.MapGet("/ThrowCustomClientException", (
    [FromQuery] HttpStatusCode? statusCode,
    [FromQuery] string? message,
    [FromQuery] string? clientMessage) =>
{
    string emptyMessage = "No Message Sent";

    CustomClientException exception = (statusCode, message) switch
    {
        { statusCode: null, message: null } => new CustomClientException(emptyMessage) { ClientErrorMessage = clientMessage },

        { statusCode: null } => new CustomClientException(message) { ClientErrorMessage = clientMessage },

        { message: null } => new CustomClientException(statusCode.Value, emptyMessage) { ClientErrorMessage = clientMessage },

        _ => new CustomClientException(statusCode.Value, message) { ClientErrorMessage = clientMessage }
    };

    throw exception;
})
.WithOpenApi();

app.Run();

// Used for integration test web application factory accessibility
public partial class Program
{
    public const string CORRELATION_HEADER_KEY = "Test-Correlation-Id";
}

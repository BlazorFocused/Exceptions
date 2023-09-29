// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using BlazorFocused.Exceptions.Middleware.ApplicationBuilder;
using BlazorFocused.Exceptions.Middleware.ExceptionBuilder;
using BlazorFocused.Testing.Logging;

namespace BlazorFocused.Exceptions.Middleware.Test.ApplicationBuilder;

public partial class ApplicationBuilderMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldReturnAbstractExceptionDetails()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        HttpStatusCode expectedStatusCode = new Faker().PickRandom<HttpStatusCode>();
        string expectedMessage = new Faker().Lorem.Sentence();
        var thrownException = new TestException(expectedStatusCode, expectedMessage);

        // IOptionsMonitor should not be called
        // This will cause a null reference exception and fail test if called
        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> emptyOptionsMonitor = default;

        requestDelegateMock.Setup(request =>
                request.Invoke(httpContext))
                    .ThrowsAsync(thrownException);

        Exception actualException =
            await Record.ExceptionAsync(() =>
                applicationBuilderMiddleware.Invoke(
                    httpContext,
                    Options.Create(new ExceptionsMiddlewareOptions()),
                    emptyOptionsMonitor,
                    NullLogger<ApplicationBuilderMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        Assert.Equal(expectedMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal((int)expectedStatusCode, actualErrorResponse.Status);
        Assert.Equal((int)expectedStatusCode, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }

    [Fact]
    public async Task Invoke_ShouldReturnClientMessageInsteadOfExceptionMessage()
    {
        // Add Test Logger to ensure internal message is being logged
        ITestLogger<ApplicationBuilderMiddleware> loggerMock =
            TestLoggerBuilder.CreateTestLogger<ApplicationBuilderMiddleware>();

        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        HttpStatusCode expectedStatusCode = new Faker().PickRandom<HttpStatusCode>();
        string expectedMessage = new Faker().Lorem.Sentence();
        string internalLogMessage = new Faker().Lorem.Sentence();
        string externalClientMessage = new Faker().Lorem.Sentence();

        var thrownException = new TestException(expectedStatusCode, internalLogMessage)
        {
            ClientErrorMessage = externalClientMessage
        };

        // IOptionsMonitor should not be called
        // This will cause a null reference exception and fail test if called
        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> emptyOptionsMonitor = default;

        requestDelegateMock.Setup(request =>
                request.Invoke(httpContext))
                    .ThrowsAsync(thrownException);

        Exception actualException =
            await Record.ExceptionAsync(() =>
                applicationBuilderMiddleware.Invoke(
                    httpContext,
                    Options.Create(new ExceptionsMiddlewareOptions()),
                    emptyOptionsMonitor,
                    loggerMock));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check only client message is seen by consumer
        Assert.Equal(externalClientMessage, actualErrorResponse.Detail);

        // Verify original exception message was logged
        loggerMock.VerifyWasCalledWith(LogLevel.Error, internalLogMessage);

        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal((int)expectedStatusCode, actualErrorResponse.Status);
        Assert.Equal((int)expectedStatusCode, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }
}

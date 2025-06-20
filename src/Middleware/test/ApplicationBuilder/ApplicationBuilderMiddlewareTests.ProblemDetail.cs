// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using BlazorFocused.Exceptions.Middleware.ApplicationBuilder;
using BlazorFocused.Exceptions.Middleware.ExceptionBuilder;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;

namespace BlazorFocused.Exceptions.Middleware.Test.ApplicationBuilder;

public partial class ApplicationBuilderMiddlewareTests
{
    [Theory]
    [MemberData(nameof(Exceptions))]
    public async Task Invoke_ShouldReturnProblemDetailsNoConfiguration(Exception thrownException)
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> optionsMonitor = default;
        var exceptionsMiddlewareOptions = new ExceptionsMiddlewareOptions();

        requestDelegateMock.Setup(request =>
                request.Invoke(httpContext))
            .ThrowsAsync(thrownException);

        Exception actualException =
            await Record.ExceptionAsync(() =>
                applicationBuilderMiddleware.Invoke(
                    httpContext,
                    Options.Create(exceptionsMiddlewareOptions),
                    optionsMonitor,
                    NullLogger<ApplicationBuilderMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        Assert.Equal(exceptionsMiddlewareOptions.DefaultErrorMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal((int)exceptionsMiddlewareOptions.DefaultErrorStatusCode, actualErrorResponse.Status);
        Assert.Equal((int)exceptionsMiddlewareOptions.DefaultErrorStatusCode, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }

    [Theory]
    [MemberData(nameof(ExceptionsWithStatusCode))]
    public async Task Invoke_ShouldReturnProblemDetailsWithDefaultMessage(Exception thrownException,
        HttpStatusCode expectedStatusCode)
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        string expectedMessage = new Faker().Lorem.Sentence();

        var configuredExceptionOptions = new ExceptionsMiddlewareBuilderOptions
        {
            ExceptionType = thrownException.GetType(),
            HttpStatusCode = expectedStatusCode,
            DefaultMessage = expectedMessage
        };

        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> optionsMonitor =
            GenerateOptionsMonitor(thrownException.GetType().AssemblyQualifiedName, configuredExceptionOptions);

        requestDelegateMock.Setup(request =>
                request.Invoke(httpContext))
            .ThrowsAsync(thrownException);

        Exception actualException =
            await Record.ExceptionAsync(() =>
                applicationBuilderMiddleware.Invoke(
                    httpContext,
                    Options.Create(new ExceptionsMiddlewareOptions()),
                    optionsMonitor,
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
    public async Task Invoke_ShouldReturnProblemDetailsWithConfiguredMessage()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        string expectedMessage = new Faker().Lorem.Sentence();
        string exceptionMessage = new Faker().Lorem.Sentence();
        var thrownException = new AccessViolationException(exceptionMessage);
        HttpStatusCode expectedStatusCode = HttpStatusCode.SwitchingProtocols;

        var configuredExceptionOptions = new ExceptionsMiddlewareBuilderOptions
        {
            ExceptionType = thrownException.GetType(),
            ExceptionResponseResolver =
                new ExceptionResponseResolver<Exception>((httpContext, exception) =>
                {
                    return new ExceptionsMiddlewareResponse
                    {
                        ErrorMessage = expectedMessage,
                        HttpStatusCode = expectedStatusCode
                    };
                })
        };

        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> optionsMonitor =
            GenerateOptionsMonitor(thrownException.GetType().AssemblyQualifiedName, configuredExceptionOptions);

        requestDelegateMock.Setup(request =>
                request.Invoke(httpContext))
            .ThrowsAsync(thrownException);

        Exception actualException =
            await Record.ExceptionAsync(() =>
                applicationBuilderMiddleware.Invoke(
                    httpContext,
                    Options.Create(new ExceptionsMiddlewareOptions()),
                    optionsMonitor,
                    NullLogger<ApplicationBuilderMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        Assert.Equal(expectedMessage, actualErrorResponse.Detail);
        Assert.NotEqual(exceptionMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal((int)expectedStatusCode, actualErrorResponse.Status);
        Assert.Equal((int)expectedStatusCode, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }

    [Fact]
    public async Task Invoke_ShouldAllowProblemDetailsOverride()
    {
        string exceptionMessage = new Faker().Lorem.Sentence();
        string expectedType = "Test Override 1";
        string overrideInstance = " - Test Override 2";
        string overrideMessage = "Test Override 3 ";
        string expectedDetail = overrideMessage + exceptionMessage;
        int expectedStatusCode = (int)HttpStatusCode.GatewayTimeout;
        var thrownException = new ApplicationException(exceptionMessage);

        using MemoryStream memoryStream =
            GenerateHttpContext(out string initialInstance, out HttpContext httpContext);

        string expectedInstance = initialInstance + overrideInstance;

        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> optionsMonitor = null;
        var exceptionsMiddlewareOptions = new ExceptionsMiddlewareOptions
        {
            ConfigureProblemDetails = (httpContext, exceptionMessage, problemDetails) =>
            {
                problemDetails.Detail = overrideMessage + exceptionMessage.Message;
                problemDetails.Instance = httpContext.Request.Path + httpContext.Request.QueryString + overrideInstance;
                problemDetails.Status = expectedStatusCode;
                problemDetails.Type = expectedType;

                return problemDetails;
            }
        };

        requestDelegateMock.Setup(request =>
                request.Invoke(httpContext))
            .ThrowsAsync(thrownException);

        Exception actualException =
            await Record.ExceptionAsync(() =>
                applicationBuilderMiddleware.Invoke(
                    httpContext,
                    Options.Create(exceptionsMiddlewareOptions),
                    optionsMonitor,
                    NullLogger<ApplicationBuilderMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        Assert.Equal(expectedDetail, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal(expectedStatusCode, actualErrorResponse.Status);
        Assert.Equal(expectedType, actualErrorResponse.Type);
        Assert.NotEqual(expectedStatusCode, httpContext.Response.StatusCode);
    }
}

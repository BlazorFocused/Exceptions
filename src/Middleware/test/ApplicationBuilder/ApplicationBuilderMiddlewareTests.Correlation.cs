// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using BlazorFocused.Exceptions.Middleware.ApplicationBuilder;
using BlazorFocused.Exceptions.Middleware.ExceptionBuilder;

namespace BlazorFocused.Exceptions.Middleware.Test.ApplicationBuilder;

public partial class ApplicationBuilderMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationKeyDuringError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        string expectedCorrelationHeader = new Faker().Internet.UserAgent();
        var thrownException = new Exception("Internal Error Message");

        var exceptionsMiddlewareOptions = new ExceptionsMiddlewareOptions
        {
            CorrelationKey = expectedCorrelationHeader
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
                    Options.Create(exceptionsMiddlewareOptions),
                    emptyOptionsMonitor,
                    NullLogger<ApplicationBuilderMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Key and default Guid Value
        Assert.True(httpContext.Response.Headers.ContainsKey(expectedCorrelationHeader));
        Assert.True(Guid.TryParse(httpContext.Response.Headers[expectedCorrelationHeader], out Guid _));

        Assert.Equal(exceptionsMiddlewareOptions.DefaultErrorMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal(500, actualErrorResponse.Status);
        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }

    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationKeyWithoutError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        string expectedCorrelationHeader = new Faker().Internet.UserAgent();

        var exceptionsMiddlewareOptions = new ExceptionsMiddlewareOptions
        {
            CorrelationKey = expectedCorrelationHeader
        };

        // IOptionsMonitor should not be called
        // This will cause a null reference exception and fail test if called
        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> emptyOptionsMonitor = default;

        Exception actualException =
            await Record.ExceptionAsync(() =>
                applicationBuilderMiddleware.Invoke(
                    httpContext,
                    Options.Create(exceptionsMiddlewareOptions),
                    emptyOptionsMonitor,
                    NullLogger<ApplicationBuilderMiddleware>.Instance));

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Key and default Guid Value
        Assert.True(httpContext.Response.Headers.ContainsKey(expectedCorrelationHeader));
        Assert.True(Guid.TryParse(httpContext.Response.Headers[expectedCorrelationHeader], out Guid _));
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationValueDuringError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        string expectedCorrelationValue = new Faker().Internet.IpAddress().ToString();
        var thrownException = new Exception("Internal Error Message");

        httpContext.TraceIdentifier = expectedCorrelationValue;

        var exceptionsMiddlewareOptions = new ExceptionsMiddlewareOptions
        {
            ConfigureCorrelationValue = (context) => context.TraceIdentifier
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
                    Options.Create(exceptionsMiddlewareOptions),
                    emptyOptionsMonitor,
                    NullLogger<ApplicationBuilderMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Correlation Value
        Assert.Equal(expectedCorrelationValue, httpContext.Response.Headers[exceptionsMiddlewareOptions.CorrelationKey]);

        Assert.Equal(exceptionsMiddlewareOptions.DefaultErrorMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal(500, actualErrorResponse.Status);
        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }

    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationValueWithoutError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        string expectedCorrelationValue = new Faker().Internet.IpAddress().ToString();

        httpContext.TraceIdentifier = expectedCorrelationValue;

        var exceptionsMiddlewareOptions = new ExceptionsMiddlewareOptions
        {
            ConfigureCorrelationValue = (context) => context.TraceIdentifier
        };

        // IOptionsMonitor should not be called
        // This will cause a null reference exception and fail test if called
        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> emptyOptionsMonitor = default;

        Exception actualException =
            await Record.ExceptionAsync(() =>
                applicationBuilderMiddleware.Invoke(
                    httpContext,
                    Options.Create(exceptionsMiddlewareOptions),
                    emptyOptionsMonitor,
                    NullLogger<ApplicationBuilderMiddleware>.Instance));

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Correlation Value
        Assert.Equal(expectedCorrelationValue, httpContext.Response.Headers[exceptionsMiddlewareOptions.CorrelationKey]);
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationKeyAndValueDuringError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        string expectedCorrelationHeader = new Faker().Internet.UserAgent();
        string expectedCorrelationValue = new Faker().Internet.IpAddress().ToString();
        var thrownException = new Exception("Internal Error Message");

        httpContext.TraceIdentifier = expectedCorrelationValue;

        var exceptionsMiddlewareOptions = new ExceptionsMiddlewareOptions
        {
            CorrelationKey = expectedCorrelationHeader,
            ConfigureCorrelationValue = (context) => context.TraceIdentifier
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
                    Options.Create(exceptionsMiddlewareOptions),
                    emptyOptionsMonitor,
                    NullLogger<ApplicationBuilderMiddleware>.Instance));

        ProblemDetails actualErrorResponse = await GetErrorResponseFromBody<ProblemDetails>(memoryStream);

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Correlation Value
        Assert.Equal(expectedCorrelationValue, httpContext.Response.Headers[expectedCorrelationHeader]);

        Assert.Equal(exceptionsMiddlewareOptions.DefaultErrorMessage, actualErrorResponse.Detail);
        Assert.Equal(expectedInstance, actualErrorResponse.Instance);
        Assert.Equal(500, actualErrorResponse.Status);
        Assert.Equal(500, httpContext.Response.StatusCode);
        Assert.Equal(thrownException.GetType().Name, actualErrorResponse.Type);
    }

    [Fact]
    public async Task Invoke_ShouldReturnConfiguredCorrelationKeyAndValueWithoutError()
    {
        using MemoryStream memoryStream =
            GenerateHttpContext(out string expectedInstance, out HttpContext httpContext);

        string expectedCorrelationHeader = new Faker().Internet.UserAgent();
        string expectedCorrelationValue = new Faker().Internet.IpAddress().ToString();

        httpContext.TraceIdentifier = expectedCorrelationValue;

        var exceptionsMiddlewareOptions = new ExceptionsMiddlewareOptions
        {
            CorrelationKey = expectedCorrelationHeader,
            ConfigureCorrelationValue = (context) => context.TraceIdentifier
        };

        // IOptionsMonitor should not be called
        // This will cause a null reference exception and fail test if called
        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> emptyOptionsMonitor = default;

        Exception actualException =
            await Record.ExceptionAsync(() =>
                applicationBuilderMiddleware.Invoke(
                    httpContext,
                    Options.Create(exceptionsMiddlewareOptions),
                    emptyOptionsMonitor,
                    NullLogger<ApplicationBuilderMiddleware>.Instance));

        // Should be null since error is caught
        actualException.Should().BeNull();

        // Check Correlation Value
        Assert.Equal(expectedCorrelationValue, httpContext.Response.Headers[expectedCorrelationHeader]);
        Assert.Equal(200, httpContext.Response.StatusCode);
    }
}

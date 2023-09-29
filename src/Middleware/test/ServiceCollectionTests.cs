// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;

namespace BlazorFocused.Exceptions.Middleware.Test;

public class ServiceCollectionTests
{
    [Fact]
    public void AddExceptionsMiddleware_ShouldEstablishBaseOptions()
    {
        var expectedMiddlewareOptions = new ExceptionsMiddlewareOptions();

        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddExceptionsMiddleware();

        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        ExceptionsMiddlewareOptions actualMiddlewareOptions =
            serviceProvider.GetRequiredService<IOptions<ExceptionsMiddlewareOptions>>().Value;

        actualMiddlewareOptions.Should().BeEquivalentTo(expectedMiddlewareOptions);
    }

    [Fact]
    public void AddExceptionsMiddleware_ShouldChangeBaseOptions()
    {
        string defaultErrorMessage = "This is a test error message";
        HttpStatusCode defaultErrorHttpStatusCode = HttpStatusCode.GatewayTimeout;
        string correlationHeaderKey = "X-Test-Header";

        var expectedMiddlewareOptions = new ExceptionsMiddlewareOptions
        {
            DefaultErrorMessage = defaultErrorMessage,
            DefaultErrorStatusCode = defaultErrorHttpStatusCode,
            CorrelationKey = correlationHeaderKey,
            ConfigureCorrelationValue = ConfigureCorrelation
        };

        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddExceptionsMiddleware(options =>
        {
            options.DefaultErrorMessage = defaultErrorMessage;
            options.DefaultErrorStatusCode = defaultErrorHttpStatusCode;
            options.CorrelationKey = correlationHeaderKey;
            options.ConfigureCorrelationValue = ConfigureCorrelation;
        });

        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        ExceptionsMiddlewareOptions actualMiddlewareOptions =
            serviceProvider.GetRequiredService<IOptions<ExceptionsMiddlewareOptions>>().Value;

        actualMiddlewareOptions.Should().BeEquivalentTo(expectedMiddlewareOptions);

        static string ConfigureCorrelation(HttpContext httpContext)
        {
            return "Configure This Value From HttpContext";
        }
    }
}

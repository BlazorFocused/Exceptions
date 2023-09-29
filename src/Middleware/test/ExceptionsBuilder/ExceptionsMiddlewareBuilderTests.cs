// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using BlazorFocused.Exceptions.Middleware.ExceptionBuilder;

namespace BlazorFocused.Exceptions.Middleware.Test.ExceptionsBuilder;

public class ExceptionsMiddlewareBuilderTests
{
    private readonly IServiceCollection serviceCollection;

    private readonly IExceptionsMiddlewareBuilder exceptionsMiddlewareBuilder;

    public ExceptionsMiddlewareBuilderTests()
    {
        serviceCollection = new ServiceCollection();

        exceptionsMiddlewareBuilder = new ExceptionsMiddlewareBuilder(serviceCollection);
    }

    [Fact]
    public void AddException_ShouldRegisterExceptionOptions()
    {
        HttpStatusCode expectedStatusCode = GenerateRandomStatusCode();
        string exceptionKey = typeof(ArgumentOutOfRangeException).AssemblyQualifiedName;

        // Under Test
        exceptionsMiddlewareBuilder.AddException<ArgumentOutOfRangeException>(expectedStatusCode);

        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> optionsMonitor =
            serviceProvider.GetService<IOptionsMonitor<ExceptionsMiddlewareBuilderOptions>>();

        ExceptionsMiddlewareBuilderOptions actualOptions = optionsMonitor.Get(exceptionKey);

        Assert.NotNull(actualOptions);

        Assert.Equal(expectedStatusCode, actualOptions.HttpStatusCode);
        Assert.Equal(exceptionKey, actualOptions.ExceptionType.AssemblyQualifiedName);

        Assert.Null(actualOptions.DefaultMessage);
        Assert.Null(actualOptions.ExceptionResponseResolver);
    }

    [Fact]
    public void AddException_ShouldRegisterExceptionOptionsWithDefaultMessage()
    {
        HttpStatusCode expectedStatusCode = GenerateRandomStatusCode();
        string exceptionKey = typeof(ArgumentException).AssemblyQualifiedName;
        string expectedResponseMessage = new Faker().Lorem.Sentence();

        // Under Test
        exceptionsMiddlewareBuilder.AddException<ArgumentException>(expectedStatusCode, expectedResponseMessage);

        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> optionsMonitor =
            serviceProvider.GetService<IOptionsMonitor<ExceptionsMiddlewareBuilderOptions>>();

        ExceptionsMiddlewareBuilderOptions actualOptions = optionsMonitor.Get(exceptionKey);

        Assert.NotNull(actualOptions);

        Assert.Equal(expectedStatusCode, actualOptions.HttpStatusCode);
        Assert.Equal(expectedResponseMessage, actualOptions.DefaultMessage);
        Assert.Equal(exceptionKey, actualOptions.ExceptionType.AssemblyQualifiedName);

        Assert.Null(actualOptions.ExceptionResponseResolver);
    }

    [Fact]
    public void AddException_ShouldRegisterExceptionOptionsWithResponseCompilation()
    {
        HttpStatusCode expectedStatusCode = GenerateRandomStatusCode();
        Type exceptionType = typeof(AppDomainUnloadedException);
        string exceptionKey = exceptionType.AssemblyQualifiedName;
        var expectedException = new AppDomainUnloadedException("This is a test");
        Mock<HttpContext> httpContextMock = new();
        Mock<HttpResponse> httpResponseMock = new();

        Func<HttpContext, AppDomainUnloadedException, ExceptionsMiddlewareResponse> responseMapping =
            GenerateExceptionsMiddlewareResponse<AppDomainUnloadedException>;

        httpContextMock.SetupGet(context => context.Response).Returns(httpResponseMock.Object);
        httpResponseMock.SetupGet(request => request.StatusCode).Returns((int)expectedStatusCode);

        // Under Test
        exceptionsMiddlewareBuilder.AddException<AppDomainUnloadedException>(responseMapping);

        IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> optionsMonitor = serviceProvider.GetService<IOptionsMonitor<ExceptionsMiddlewareBuilderOptions>>();
        ExceptionsMiddlewareBuilderOptions actualOptions = optionsMonitor.Get(exceptionKey);

        Assert.NotNull(actualOptions);

        Assert.NotNull(actualOptions.ExceptionResponseResolver);
        Assert.Equal(exceptionKey, actualOptions.ExceptionType.AssemblyQualifiedName);

        ExceptionsMiddlewareResponse expectedExceptionsResponse =
            GenerateExceptionsMiddlewareResponse(httpContextMock.Object, expectedException);

        var resolver = actualOptions.ExceptionResponseResolver as ExceptionResponseResolver<AppDomainUnloadedException>;

        ExceptionsMiddlewareResponse actualExceptionsResponse =
            resolver.Resolve(httpContextMock.Object, expectedException);

        Assert.Equal(expectedExceptionsResponse.HttpStatusCode, actualExceptionsResponse.HttpStatusCode);
        Assert.Equal(expectedExceptionsResponse.ErrorMessage, actualExceptionsResponse.ErrorMessage);
    }

    private static HttpStatusCode GenerateRandomStatusCode() =>
        new Faker().PickRandom<HttpStatusCode>();

    private static ExceptionsMiddlewareResponse GenerateExceptionsMiddlewareResponse<T>(HttpContext httpContext, T exception)
        where T : Exception =>
            new()
            {
                HttpStatusCode = (HttpStatusCode)httpContext.Response.StatusCode,
                ErrorMessage = exception.Message
            };
}

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
using System.Text;
using System.Text.Json;
using BlazorFocused.Exceptions.Middleware.ApplicationBuilder;
using BlazorFocused.Exceptions.Middleware.ExceptionBuilder;
using Xunit.Sdk;

namespace BlazorFocused.Exceptions.Middleware.Test.ApplicationBuilder;

public partial class ApplicationBuilderMiddlewareTests
{
    private readonly Mock<RequestDelegate> requestDelegateMock;

    private readonly ApplicationBuilderMiddleware applicationBuilderMiddleware;

    public ApplicationBuilderMiddlewareTests()
    {
        requestDelegateMock = new();

        applicationBuilderMiddleware = new ApplicationBuilderMiddleware(requestDelegateMock.Object);
    }

    private static async Task<T> GetErrorResponseFromBody<T>(MemoryStream stream)
    {
        stream.Position = 0;
        using var streamReader = new StreamReader(stream);
        string errorResponseString = await streamReader.ReadToEndAsync();

        try
        {
            return JsonSerializer.Deserialize<T>(errorResponseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            throw new XunitException($"Response was not in json -> Response: '{errorResponseString}'");
        }
    }

    // Adding real options monitor due to inability to Moq
    private static IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> GenerateOptionsMonitor(string name, ExceptionsMiddlewareBuilderOptions options)
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection
            .AddOptions<ExceptionsMiddlewareBuilderOptions>(name)
            .Configure(internalOptions =>
        {
            internalOptions.ExceptionType = options.ExceptionType;
            internalOptions.HttpStatusCode = options.HttpStatusCode;
            internalOptions.DefaultMessage = options.DefaultMessage;
            internalOptions.ExceptionResponseResolver = options.ExceptionResponseResolver;
        });

        return serviceCollection
            .BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<ExceptionsMiddlewareBuilderOptions>>();
    }

    private record UrlPathAndQuery(string Url, string QueryString);

    private static MemoryStream GenerateHttpContext(out string instance, out HttpContext httpContext)
    {
        UrlPathAndQuery urlAndPathQuery = GenerateRelativeUrlPathAndQuery();
        instance = string.Concat(urlAndPathQuery.Url, urlAndPathQuery.QueryString);

        var memoryStream = new MemoryStream();
        httpContext = new DefaultHttpContext();
        httpContext.Response.Body = memoryStream;
        httpContext.Request.Path = urlAndPathQuery.Url;
        httpContext.Request.QueryString = new QueryString(urlAndPathQuery.QueryString);

        return memoryStream;
    }

    private static UrlPathAndQuery GenerateRelativeUrlPathAndQuery()
    {
        string url = new Faker().Internet.UrlRootedPath();
        var parameterBuilder = new StringBuilder("?");
        int paramSize = new Faker().Random.Int(2, 5);
        List<UrlParameter> urlParameters = GenerateUrlParameterSet(paramSize);

        for (int i = 0; i < paramSize; i++)
        {
            UrlParameter urlParameter = urlParameters[i];

            parameterBuilder.Append($"&{urlParameter.ParamName}={urlParameter.ParamValue}");
        }

        string parameters = parameterBuilder.ToString();

        return new UrlPathAndQuery(Url: url, QueryString: parameters);
    }

    private static List<UrlParameter> GenerateUrlParameterSet(int count) =>
        new Faker<UrlParameter>()
        .RuleFor(parameter => parameter.ParamName, fake => fake.Commerce.Product())
        .RuleFor(parameter => parameter.ParamValue, fake => fake.Commerce.ProductMaterial())
        .Generate(count);

    public static TheoryData<Exception, HttpStatusCode> ExceptionsWithStatusCode => new()
    {
        {
            new Exception("Exception Message Test"),
            HttpStatusCode.InternalServerError
        },
        {
            new ApplicationException("Exception Message Test 2"),
            HttpStatusCode.BadGateway
        },
        {
            new DirectoryNotFoundException(),
            HttpStatusCode.Locked
        }
    };

    public static TheoryData<Exception> Exceptions => new()
    {
        {
            new Exception("Exception Message Test")
        },
        {
            new ApplicationException("Exception Message Test 2")
        },
        {
            new DirectoryNotFoundException()
        }
    };

    private class TestException : ExceptionsMiddlewareException
    {
        private const HttpStatusCode defaultStatusCode = HttpStatusCode.Unused;

        public TestException(string message) :
            base(defaultStatusCode, message)
        { }

        public TestException(HttpStatusCode httpStatusCode, string message) :
            base(httpStatusCode, message)
        { }

        public TestException(string message, Exception innerException) :
            base(defaultStatusCode, message, innerException)
        { }

        public TestException(HttpStatusCode httpStatusCode, string message, Exception innerException) :
            base(httpStatusCode, message, innerException)
        { }
    }

    private class UrlParameter
    {
        public string ParamName { get; set; }

        public string ParamValue { get; set; }
    }
}

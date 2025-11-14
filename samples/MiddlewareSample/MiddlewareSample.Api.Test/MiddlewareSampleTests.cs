// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using MiddlewareSample.Api.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using BlazorFocused.Exceptions.Middleware;

namespace MiddlewareSample.Api.Test;

[Collection(nameof(MiddlewareSampleTestCollection))]
public class MiddlewareSampleTests
{
    private readonly HttpClient httpClient;
    private readonly ITestOutputHelper testOutputHelper;

    public MiddlewareSampleTests(WebApplicationFactory<Program> webApplicationFactory, ITestOutputHelper testOutputHelper)
    {
        httpClient = webApplicationFactory.CreateClient();

        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task ShouldReturnProperStatusCodes()
    {
        HttpStatusCode expectedStatusCode = GenerateNon300LevelStatusCode();

        testOutputHelper.WriteLine("Expected Status Code: {0}", expectedStatusCode);

        string url = $"/ThrowCustomClientException?statusCode={(int)expectedStatusCode}";

        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url, TestContext.Current.CancellationToken);

        testOutputHelper.WriteLine("Expected Status Code: {0}", httpResponseMessage.StatusCode);

        Assert.Equal(expectedStatusCode, httpResponseMessage.StatusCode);

        ValidateHeaders(httpResponseMessage.Headers);
    }

    [Fact]
    public async Task ShouldReturnAmbiguousForRandomException()
    {
        string url = "/ThrowRandomException";

        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url, TestContext.Current.CancellationToken);

        ProblemDetails problemDetails = await GetProblemDetailsAsync(httpResponseMessage);

        Assert.Equal(nameof(RandomException), problemDetails.Type);
        Assert.Equal((int)HttpStatusCode.FailedDependency, problemDetails.Status);
        Assert.Equal(HttpStatusCode.FailedDependency.ToString(), problemDetails.Title);
        Assert.Equal(new ExceptionsMiddlewareOptions().DefaultErrorMessage, problemDetails.Detail);
        Assert.Equal(url, problemDetails.Instance);

        ValidateHeaders(httpResponseMessage.Headers);
    }

    [Fact]
    public async Task ShouldReturnRuntimeExceptionStatusCodeException()
    {
        HttpStatusCode expectedStatusCode = GenerateNon300LevelStatusCode();
        string expectedMessage = new Faker().Lorem.Sentence();

        string url = $"/ThrowCustomClientException?statusCode={(int)expectedStatusCode}&message={expectedMessage}";

        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url, TestContext.Current.CancellationToken);

        ProblemDetails problemDetails = await GetProblemDetailsAsync(httpResponseMessage);

        Assert.Equal(nameof(CustomClientException), problemDetails.Type);
        Assert.Equal((int)expectedStatusCode, problemDetails.Status);
        Assert.Equal(expectedStatusCode.ToString(), problemDetails.Title);
        Assert.Equal(expectedMessage, problemDetails.Detail);
        Assert.Equal(url, HttpUtility.UrlDecode(problemDetails.Instance));

        ValidateHeaders(httpResponseMessage.Headers);
    }

    [Fact]
    public async Task ShouldReturnClientSpecificExceptionMessage()
    {
        HttpStatusCode expectedStatusCode = GenerateNon300LevelStatusCode();
        string internalExceptionMessage = new Faker().Lorem.Sentence();
        string expectedClientMessage = new Faker().Lorem.Sentence();

        string url = $"/ThrowCustomClientException?statusCode={(int)expectedStatusCode}&message={internalExceptionMessage}&clientMessage={expectedClientMessage}";

        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url, TestContext.Current.CancellationToken);

        ProblemDetails problemDetails = await GetProblemDetailsAsync(httpResponseMessage);

        Assert.NotEqual(internalExceptionMessage, problemDetails.Detail);
        Assert.Equal(expectedClientMessage, problemDetails.Detail);

        Assert.Equal(nameof(CustomClientException), problemDetails.Type);
        Assert.Equal((int)expectedStatusCode, problemDetails.Status);
        Assert.Equal(expectedStatusCode.ToString(), problemDetails.Title);
        Assert.Equal(url, HttpUtility.UrlDecode(problemDetails.Instance));

        ValidateHeaders(httpResponseMessage.Headers);
    }

    private static async Task<ProblemDetails> GetProblemDetailsAsync(HttpResponseMessage httpResponseMessage)
    {
        Stream responseContentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

        return JsonSerializer.Deserialize<ProblemDetails>(responseContentStream);
    }

    private static void ValidateHeaders(HttpResponseHeaders httpResponseHeaders)
    {
        Assert.True(httpResponseHeaders.TryGetValues(Program.CORRELATION_HEADER_KEY, out IEnumerable<string> headers));
        Assert.Single(headers);
        Assert.False(string.IsNullOrWhiteSpace(headers.First()));
    }

    // When running responses from WebApplication Factory, 300 level errors are
    // causing unpredicted behavior
    private static HttpStatusCode GenerateNon300LevelStatusCode()
    {
        HttpStatusCode? expectedStatusCodeGenerated;

        do
        {
            expectedStatusCodeGenerated =
                new Faker().PickRandomWithout<HttpStatusCode>(
                    HttpStatusCode.Redirect,
                    HttpStatusCode.MovedPermanently,
                    HttpStatusCode.Moved,
                    HttpStatusCode.SeeOther,
                    HttpStatusCode.NotModified,
                    HttpStatusCode.UseProxy,
                    HttpStatusCode.TemporaryRedirect,
                    HttpStatusCode.PermanentRedirect);
        }
        while (!expectedStatusCodeGenerated.HasValue ||
            ((int)expectedStatusCodeGenerated.Value > 299 && (int)expectedStatusCodeGenerated.Value < 400));

        return expectedStatusCodeGenerated.Value;
    }
}

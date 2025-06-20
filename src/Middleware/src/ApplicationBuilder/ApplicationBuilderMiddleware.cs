// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Text.Json;
using BlazorFocused.Exceptions.Middleware.ExceptionBuilder;

namespace BlazorFocused.Exceptions.Middleware.ApplicationBuilder;

internal class ApplicationBuilderMiddleware(RequestDelegate next)
{
    public async Task Invoke(
        HttpContext httpContext,
        IOptions<ExceptionsMiddlewareOptions> exceptionsMiddlewareOptions,
        IOptionsMonitor<ExceptionsMiddlewareBuilderOptions> exceptionOptionsMonitor,
        ILogger<ApplicationBuilderMiddleware> logger)
    {
        ExceptionsMiddlewareOptions exceptionsMiddlewareOptionsValue = exceptionsMiddlewareOptions.Value ??
            throw new ArgumentNullException(nameof(exceptionsMiddlewareOptions));

        string correlationKey = exceptionsMiddlewareOptionsValue.CorrelationKey;

        bool correlationKeyExists =
            httpContext.Request.Headers.TryGetValue(exceptionsMiddlewareOptionsValue.CorrelationKey, out StringValues correlationValues);

        string correlationValue = correlationKeyExists switch
        {
            { } when correlationKeyExists => correlationValues.First(),

            { } when exceptionsMiddlewareOptionsValue.ConfigureCorrelationValue is not null =>
                    exceptionsMiddlewareOptionsValue.ConfigureCorrelationValue(httpContext),

            _ => Guid.NewGuid().ToString()
        };

        var scope = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationValue
        };

        using (logger.BeginScope(scope))
        {
            try
            {
                httpContext.Response.Headers[correlationKey] = correlationValue;

                await next(httpContext);
            }
            catch (ExceptionsMiddlewareException exception)
            {
                if (string.IsNullOrEmpty(exception.ClientErrorMessage))
                {
                    await LogAndWriteExceptionAsync(exception.StatusCode, exception);
                }
                else
                {
                    await LogAndWriteExceptionAsync(exception.StatusCode, exception, exception.ClientErrorMessage);
                }
            }
            catch (Exception exception)
            {
                ExceptionsMiddlewareBuilderOptions exceptionOptions =
                    exceptionOptionsMonitor?.Get(exception.GetType().AssemblyQualifiedName);

                Task logAndWriteResponseTask = exceptionOptions switch
                {
                    // Leverage Message composition
                    // If both default message and message composition exists,
                    // message composition will take precedence
                    { } when exceptionOptions is not null && exceptionOptions.ExceptionResponseResolver is not null =>
                        LogAndWriteExceptionsResponseResolverAsync(exceptionOptions.ExceptionResponseResolver, exception),

                    // Leverage Default Message configured (and status code)
                    { } when exceptionOptions is not null &&
                            exceptionOptions.DefaultMessage is not null &&
                            exceptionOptions.HttpStatusCode.HasValue =>
                        LogAndWriteExceptionAsync(
                            exceptionOptions.HttpStatusCode.Value,
                            exception,
                            exceptionOptions.DefaultMessage),

                    // Leverage Status Code configured
                    { } when exceptionOptions is not null && exceptionOptions.HttpStatusCode.HasValue =>
                        LogAndWriteExceptionAsync(
                            exceptionOptions.HttpStatusCode.Value,
                            exception,
                            exceptionsMiddlewareOptionsValue.DefaultErrorMessage),

                    _ => LogAndWriteExceptionAsync(
                            exceptionsMiddlewareOptionsValue.DefaultErrorStatusCode,
                            exception,
                            exceptionsMiddlewareOptionsValue.DefaultErrorMessage)
                }; ;

                await logAndWriteResponseTask;
            }
        }

        async Task LogAndWriteExceptionsResponseResolverAsync(ExceptionResponseResolver exceptionResponseResolver, Exception exception)
        {
            ExceptionsMiddlewareResponse exceptionsMiddlewareResponse =
                            exceptionResponseResolver.Resolve(httpContext, exception);

            await LogAndWriteExceptionAsync(
                exceptionsMiddlewareResponse.HttpStatusCode,
                exception,
                exceptionsMiddlewareResponse.ErrorMessage);
        }

        async Task LogAndWriteExceptionAsync(HttpStatusCode httpStatusCode, Exception exception, string messageOverride = default)
        {
            if (exception.Data.Count == 0)
            {
                ProblemDetails problemDetails =
                    InitializeProblemDetails<ProblemDetails>(httpStatusCode, exception, messageOverride);

                await LogAndWriteProblemDetailsExceptionAsync(problemDetails, exception);
            }
            else
            {
                ValidationProblemDetails problemDetails =
                    InitializeProblemDetails<ValidationProblemDetails>(httpStatusCode, exception, messageOverride);

                await LogAndWriteValidationProblemDetailsExceptionAsync(problemDetails, exception);
            }
        }

        T InitializeProblemDetails<T>(HttpStatusCode httpStatusCode, Exception exception, string messageOverride = default)
            where T : ProblemDetails, new()
        {
            return new T
            {
                Instance = !string.IsNullOrWhiteSpace(httpContext.Request.QueryString.ToString()) ?
                    string.Concat(httpContext.Request.Path, httpContext.Request.QueryString) :
                    httpContext.Request.Path,
                Detail = messageOverride ?? exception.Message,
                Status = (int)httpStatusCode,
                Title = httpStatusCode.ToString(),
                Type = exception.GetType().Name
            };
        }

        async Task LogAndWriteValidationProblemDetailsExceptionAsync(ValidationProblemDetails problemDetails, Exception exception)
        {
            ExceptionDataHelper.SetValidationDetailsFromExceptionData(problemDetails, exception);

            await LogAndWriteProblemDetailsExceptionAsync(problemDetails, exception);
        }

        async Task LogAndWriteProblemDetailsExceptionAsync(ProblemDetails problemDetails, Exception exception)
        {
            logger.LogError(exception, "{Status}({Title}) - {Message}", problemDetails.Status, problemDetails.Title, exception.Message);

            httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

            if (exceptionsMiddlewareOptionsValue.ConfigureProblemDetails is not null)
            {
                problemDetails = exceptionsMiddlewareOptionsValue.ConfigureProblemDetails(httpContext, exception, problemDetails);
            }

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                problemDetails.GetType(), // WriteAsJson needs type to add additional fields provided in ValidationProblemDetails
                new JsonSerializerOptions { WriteIndented = true },
                "application/json");
        }
    }
}


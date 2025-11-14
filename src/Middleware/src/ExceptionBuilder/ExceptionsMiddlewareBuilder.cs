// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace BlazorFocused.Exceptions.Middleware.ExceptionBuilder;

internal class ExceptionsMiddlewareBuilder(IServiceCollection serviceCollection) : IExceptionsMiddlewareBuilder
{
    public IExceptionsMiddlewareBuilder AddException<TException>(HttpStatusCode httpStatusCode)
        where TException : Exception
    {
        serviceCollection.AddOptions<ExceptionsMiddlewareBuilderOptions>(typeof(TException).AssemblyQualifiedName)
            .Configure(options =>
            {
                options.ExceptionType = typeof(TException);
                options.HttpStatusCode = httpStatusCode;
                options.ExceptionResponseResolver = null;
            });

        return this;
    }

    public IExceptionsMiddlewareBuilder AddException<TException>(HttpStatusCode httpStatusCode, string clientErrorMessage)
        where TException : Exception
    {
        serviceCollection.AddOptions<ExceptionsMiddlewareBuilderOptions>(typeof(TException).AssemblyQualifiedName)
            .Configure(options =>
            {
                options.ExceptionType = typeof(TException);
                options.HttpStatusCode = httpStatusCode;
                options.DefaultMessage = clientErrorMessage;
            });

        return this;
    }

    public IExceptionsMiddlewareBuilder AddException<TException>(Func<HttpContext, TException, ExceptionsMiddlewareResponse> buildExceptionResponse)
        where TException : Exception
    {
        serviceCollection.AddOptions<ExceptionsMiddlewareBuilderOptions>(typeof(TException).AssemblyQualifiedName)
            .Configure(options =>
            {
                var resolver = new ExceptionResponseResolver<TException>(buildExceptionResponse);

                options.ExceptionType = typeof(TException);
                options.ExceptionResponseResolver = resolver;
                options.DefaultMessage = null;
                options.HttpStatusCode = null;
            });

        return this;
    }
}

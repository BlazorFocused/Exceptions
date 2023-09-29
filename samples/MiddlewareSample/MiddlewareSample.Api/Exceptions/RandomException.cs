// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

namespace MiddlewareSample.Api.Exceptions;

public class RandomException : Exception
{
    public const string DEFAULT_MESSAGE = "This is a default error message if one is not configured";

    public RandomException() : base(DEFAULT_MESSAGE) { }

    public RandomException(string message) : base(message) { }

    public RandomException(string message, Exception innerException) : base(message, innerException) { }
}

// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Text.Json;

namespace BlazorFocused.Exceptions.Middleware.ExceptionBuilder;

internal static class ExceptionDataHelper
{
    public static void SetValidationDetailsFromExceptionData(ValidationProblemDetails problemDetails, Exception exception)
    {
        foreach (object key in exception.Data.Keys)
        {
            object data = exception.Data[key];

            string[] values = data switch
            {
                { } when data is string singleEntry => new string[] { singleEntry },

                { } when data is IList<string> listOfValues => listOfValues.ToArray(),

                { } when data is IList generalList => GetArrayOfStringsFromObjects(generalList),

                { } when data is not null => GetArrayOfStringsFromSingleObject(data),

                _ => Array.Empty<string>()
            };

            problemDetails.Errors.TryAdd(key.ToString(), values);
        }
    }
    private static string[] GetArrayOfStringsFromObjects(IList list)
    {
        var values = new List<string>();

        foreach (object item in list)
        {
            values.Add(JsonSerializer.Serialize(item));
        }

        return values.ToArray();
    }

    private static string[] GetArrayOfStringsFromSingleObject(object inputObject)
    {
        string value = JsonSerializer.Serialize(inputObject);

        return new string[] { value };
    }
}

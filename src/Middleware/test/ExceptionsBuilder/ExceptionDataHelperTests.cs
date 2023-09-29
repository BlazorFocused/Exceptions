// -------------------------------------------------------
// Copyright (c) BlazorFocused All rights reserved.
// Licensed under the MIT License
// -------------------------------------------------------

using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using BlazorFocused.Exceptions.Middleware.ExceptionBuilder;

namespace BlazorFocused.Exceptions.Middleware.Test.ExceptionsBuilder;

public class ExceptionDataHelperTests
{
    [Fact]
    public void SetValidationDetailsFromExceptionData_ShouldAddSingleString()
    {
        var problemDetails = new ValidationProblemDetails();
        var exception = new Exception();

        string singleStringKey = GenerateRandomString();
        string singleStringValue = GenerateRandomString();

        exception.Data.Add(singleStringKey, singleStringValue);

        ExceptionDataHelper.SetValidationDetailsFromExceptionData(problemDetails, exception);

        Assert.Single(problemDetails.Errors);
        Assert.Single(problemDetails.Errors[singleStringKey]);

        Assert.Equal(singleStringValue, problemDetails.Errors[singleStringKey].First());
    }

    [Fact]
    public void SetValidationDetailsFromExceptionData_ShouldAddArrayOfStrings()
    {
        var problemDetails = new ValidationProblemDetails();
        var exception = new Exception();

        string firstArrayKey = GenerateRandomString();
        string secondArrayKey = GenerateRandomString();

        string[] firstArrayOfStrings = new Faker().Make<string>(10, GenerateRandomString).ToArray();
        string[] secondArrayOfStrings = new Faker().Make<string>(5, GenerateRandomString).ToArray();

        exception.Data.Add(firstArrayKey, firstArrayOfStrings);
        exception.Data.Add(secondArrayKey, secondArrayOfStrings);

        ExceptionDataHelper.SetValidationDetailsFromExceptionData(problemDetails, exception);

        Assert.NotEmpty(problemDetails.Errors);
        Assert.Equal(2, problemDetails.Errors.Count);

        problemDetails.Errors[firstArrayKey].Should().BeEquivalentTo(firstArrayOfStrings);
        problemDetails.Errors[secondArrayKey].Should().BeEquivalentTo(secondArrayOfStrings);
    }

    [Fact]
    public void SetValidationDetailsFromExceptionData_ShouldAddListOfStrings()
    {
        var problemDetails = new ValidationProblemDetails();
        var exception = new Exception();

        string firstArrayKey = GenerateRandomString();
        string secondArrayKey = GenerateRandomString();

        IList<string> firstArrayOfStrings = new Faker().Make<string>(10, GenerateRandomString);
        IList<string> secondArrayOfStrings = new Faker().Make<string>(5, GenerateRandomString);

        exception.Data.Add(firstArrayKey, firstArrayOfStrings);
        exception.Data.Add(secondArrayKey, secondArrayOfStrings);

        ExceptionDataHelper.SetValidationDetailsFromExceptionData(problemDetails, exception);

        Assert.NotEmpty(problemDetails.Errors);
        Assert.Equal(2, problemDetails.Errors.Count);

        problemDetails.Errors[firstArrayKey].Should().BeEquivalentTo(firstArrayOfStrings);
        problemDetails.Errors[secondArrayKey].Should().BeEquivalentTo(secondArrayOfStrings);
    }

    [Fact]
    public void SetValidationDetailsFromExceptionData_ShouldAddSingleObject()
    {
        var problemDetails = new ValidationProblemDetails();
        var exception = new Exception();

        string inputObjectKey = GenerateRandomString();
        var inputObject = new TestObjectClass();
        string inputObjectString = JsonSerializer.Serialize(inputObject);

        exception.Data.Add(inputObjectKey, inputObject);

        ExceptionDataHelper.SetValidationDetailsFromExceptionData(problemDetails, exception);

        Assert.Single(problemDetails.Errors);
        Assert.Single(problemDetails.Errors[inputObjectKey]);

        Assert.Equal(inputObjectString, problemDetails.Errors[inputObjectKey].First());
    }

    [Fact]
    public void SetValidationDetailsFromExceptionData_ShouldAddArrayOfSerializedObjects()
    {
        var problemDetails = new ValidationProblemDetails();
        var exception = new Exception();

        string inputObjectKey = GenerateRandomString();
        var inputObjectOne = new TestObjectClass();
        var inputObjectTwo = new TestObjectClass();
        string inputObjectOneString = JsonSerializer.Serialize(inputObjectOne);
        string inputObjectTwoString = JsonSerializer.Serialize(inputObjectTwo);

        exception.Data.Add(inputObjectKey, new TestObjectClass[] { inputObjectOne, inputObjectTwo });

        ExceptionDataHelper.SetValidationDetailsFromExceptionData(problemDetails, exception);

        Assert.Single(problemDetails.Errors);
        Assert.Equal(2, problemDetails.Errors[inputObjectKey].Length);

        Assert.Equal(inputObjectOneString, problemDetails.Errors[inputObjectKey].ElementAt(0));
        Assert.Equal(inputObjectTwoString, problemDetails.Errors[inputObjectKey].ElementAt(1));
    }

    [Fact]
    public void SetValidationDetailsFromExceptionData_ShouldAddListOfSerializedObjects()
    {
        var problemDetails = new ValidationProblemDetails();
        var exception = new Exception();

        string inputObjectKey = GenerateRandomString();
        var inputObjectOne = new TestObjectClass();
        var inputObjectTwo = new TestObjectClass();
        string inputObjectOneString = JsonSerializer.Serialize(inputObjectOne);
        string inputObjectTwoString = JsonSerializer.Serialize(inputObjectTwo);

        exception.Data.Add(inputObjectKey, new List<TestObjectClass> { inputObjectOne, inputObjectTwo });

        ExceptionDataHelper.SetValidationDetailsFromExceptionData(problemDetails, exception);

        Assert.Single(problemDetails.Errors);
        Assert.Equal(2, problemDetails.Errors[inputObjectKey].Length);

        Assert.Equal(inputObjectOneString, problemDetails.Errors[inputObjectKey].ElementAt(0));
        Assert.Equal(inputObjectTwoString, problemDetails.Errors[inputObjectKey].ElementAt(1));
    }

    private string GenerateRandomString() =>
        new Faker().Random.AlphaNumeric(new Faker().Random.Int(10, 20));

    private class TestObjectClass
    {
        public string FieldOne { get; set; } = new Faker().Lorem.Sentence();

        public string FieldTwo { get; set; } = new Faker().Lorem.Paragraph();
    }
}

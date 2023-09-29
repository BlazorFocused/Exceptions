[![Nuget Version](https://img.shields.io/nuget/v/BlazorFocused.Exceptions.Middleware?logo=nuget)](https://www.nuget.org/packages/BlazorFocused.Exceptions.Middleware)
[![Nuget Downloads](https://img.shields.io/nuget/dt/BlazorFocused.Exceptions.Middleware?logo=nuget)](https://www.nuget.org/packages/BlazorFocused.Exceptions.Middleware)

# BlazorFocused.Exceptions.Middleware

Exceptions Middleware allows applications to determine status codes and error messages for exceptions thrown in the application. This allows the application to be more flexible in how it handles exceptions and allows the application to be more consistent in how it handles exceptions.

## NuGet Packages

| Package                                                                                                    | Description                                                                    |
| ---------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| [BlazorFocused.Exceptions.Middleware](https://www.nuget.org/packages/BlazorFocused.Exceptions.Middleware/) | Standardizing Exception Handling Middleware to enhance API Consumer experience |

## Documentation

Please visit the [BlazorFocused.Exceptions Documentation Site](https://BlazorFocused.github.io/Exceptions/) for installation, getting started, and API documentation.

## Samples

Please visit and/or download our [BlazorFocused.Exceptions.Middleware Sample Solution](https://github.com/BlazorFocused/Exceptions/tree/main/samples/projectsample) to get a more in-depth view of usage.

## Installation

.NET CLI

```dotnetcli

dotnet add package BlazorFocused.Exceptions.Middleware

```

## Quick Start

### Configuration

Register Exception Status Codes (Program.cs or Startup.cs)

```csharp
builder.Services
    .AddExceptionsMiddleware()
        .AddException<YourCustomOrBuiltInException>(HttpStatusCode.GatewayTimeout);
```

Activate Exception Handling Middleware (Program.cs or Startup.cs)

```csharp
app.UseExceptionsMiddleware();
```

### Behavior

With the setup in the previous section, now throwing your `YourCustomOrBuiltInException` exception type will generate the following output in the response body:

Exception Code:

```csharp
throw new YourCustomOrBuiltInException("This Message Will Get Logged And Returned");
```

Http Response Body (ProblemDetails):

```json
{
  "type": "YourCustomOrBuiltInException",
  "title": "GatewayTimeout",
  "status": 504,
  "detail": "This Message Will Get Logged And Returned",
  "instance": "/api/YourEndpoint/PathAndQueryRequest?IncludingParams=true",
  "correlationId": "782e230a-7fb7-43aa-8d64-56d27c9f0e27"
}
```

ADDITIONALLY, adding `Exception.Data` to show multiple errors will be returned as below:

Exception Code:

```csharp
var exceptionToBeThrown = new YourCustomOrBuiltInException("This Message Will Get Logged And Returned");

exceptionToBeThrown.Data.Add("TestField1", "This Failed for some reason");
exceptionToBeThrown.Data.Add("TestField2", "This Failed for some other reason");
exceptionToBeThrown.Data.Add("TestField3", "This Failed for another reason");

throw exceptionToBeThrown;
```

Http Response Body (ValidationProblemDetails):

```json
{
  "type": "YourCustomOrBuiltInException",
  "title": "GatewayTimeout",
  "status": 504,
  "detail": "This Message Will Get Logged And Returned",
  "instance": "/api/YourEndpoint/PathAndQueryRequest?IncludingParams=true",
  "correlationId": "da3f8dde-e4a0-4932-9508-36e52cbad480",
  "errors": {
    "TestField1": ["This Failed for some reason"],
    "TestField2": ["This Failed for some other reason"],
    "TestField3": ["This Failed for another reason"]
  }
}
```

Full Setup Sample (Program.cs)

```csharp
using ExceptionsAPI;
using System.Net;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom exceptions and status codes
builder.Services
    .AddExceptionsAPI()
        // Chain multiple exceptions to the same status code to various status codes
        .AddException<YourCustomOrBuiltInException>(HttpStatusCode.GatewayTimeout);

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// Register Exceptions API Middleware
app.UseExceptionsAPI();
```

## Status Code & Error Message Runtime Determination

You can establish specific status codes and error messages for specific exceptions at runtime. This allows you to have a single exception type that can be thrown in multiple places and have different status codes and error messages depending on the context.

1. Create a class that inherits from the abstract class `ExceptionsMiddlewareException`. This class will allow an input of a status code as well as an optional client error message (initialized during exception creation).

   ```csharp
   public class CustomClientException : ExceptionsMiddlewareException
   {
       private const HttpStatusCode defaultStatusCode = HttpStatusCode.Conflict;

       // You can set a constructor to pass a default status code
       public CustomClientException(string message) :
           base(defaultStatusCode, message)
       { }

       // You can create a constructor to receive a status code dynamically
       public CustomClientException(HttpStatusCode httpStatusCode, string message) :
           base(httpStatusCode, message)
       { }

       public CustomClientException(string message, Exception innerException) :
           base(defaultStatusCode, message, innerException)
       { }

       public CustomClientException(HttpStatusCode httpStatusCode, string message, Exception innerException) :
           base(httpStatusCode, message, innerException)
       { }
   }
   ```

1. Throwing the below exception will result in the following response body:

   ```csharp
   throw new CustomClientException(HttpStatusCode.BadRequest, "Internal Message")
   {
       // "Internal Message" will be logged
       // "External Message" will be returned
       // "Internal Message" will be returned if ClientErrorMessage is not set
       ClientErrorMessage = "External Message"
   }
   ```

   Output:

   ```json
   {
     "type": "CustomClientException",
     "title": "BadRequest",
     "status": 400,
     "detail": "External Message",
     "instance": "/api/YourEndpoint/PathAndQueryRequest?IncludingParams=true"
   }
   ```

## Correlation IDs

By default, a correlation ID is generated for each request (random Guid in header key `X-Correlation-ID`). This correlation ID is included in the response body. This correlation ID can be used to trace the request through the logs. To change this behavior, you can set the `ExceptionsAPIOptions` in the `AddExceptionsAPI` method.

```csharp
serviceCollection.AddExceptionsMiddleware(options =>
{
    options.CorrelationKey = "Use-This-Correlation-Key";

    // Option to derive from HttpContext
    options.ConfigureCorrelationValue = (httpContext) => httpContext.TraceIdentifier;
});
```

# Audacia.Log.AspNetCore

Standardized logging configuration for Audacia ASP.NET Core Web projects using Application Insights.

This is a standalone library (from v2.0.0 onwards) and is not dependent on Audacia.Log or Serilog. Please remove them from your web application when upgrading as going forwards the preferred approach is to use purely Application Insights and Microsoft logging abstractions.

## Usage

Copy the following json into your `appsettings.json` file. Configure the Instrumentation Key and  the QuickPulseApi Key to enable application insights for your app service.

 - For more information on Application Insights see, https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core
 - For more information on QuickPulse Metrics see, https://docs.microsoft.com/en-us/azure/azure-monitor/app/live-stream#secure-the-control-channel

The default logging providers are Debug, Console, EventLog, EventSource, ApplicationInsights. LogLevel on its own applies to all providers. Log levels can be altered by specifying the assembly name and log level for a logging provider.

```json
{
  "ApplicationInsights": {
    "ConnectionString": "",
    "EnableAdaptiveSampling": false,
    "QuickPulseApiKey": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    },
    "EventLog": {
      "Default": "Warning"
    },
    "ApplicationInsights": {
      "IncludeScopes": true,
      "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Warning",
        "IdentityServer4": "Information",
        "IdentityServer4.Validation.TokenValidator": "Warning"
      }
    }
  }
}
```

To configure application insights and quick pulse metrics add the following code to your `Startup.cs`.

```csharp
using Audacia.Log.AspNetCore;

public IConfiguration Configuration { get; set; }

public void ConfigureServices(IServiceCollection services)
{
 services.ConfigureApplicationInsights(Configuration);
}
```

## Optional

### ClaimsActionLogFilter
This filter can be registered to include logs for any claims at the beginning and end of each Action Method. Register it like so:

```csharp
using Audacia.Log.AspNetCore;

public IConfiguration Configuration { get; set; }

public void ConfigureServices(IServiceCollection services)
{
 services.ConfigureActionContentLogging(Configuration);
 services.AddClaimsTelemetry();
 services.AddControllers(x => x.Filters.Add<LogClaimsActionFilterAttribute>());
}
```

### Add optional CustomAdditionalClaimsTelemetryProvider
AddClaimsTelemetry() is overloaded and if you want to pass in your own implementation of "CustomAdditionalClaimsTelemetryProvider" to select which properties you would like to pass on, do it like below:

```csharp
services.AddClaimsTelemetry(new CustomAdditionalClaimsTelemetryProvider((user) => 
{
  return new List<(string Name, string Data)>
  {
    ("customproperty", user.Claims.Where(c => c.Type == "customproperty").Single().Value)
  };
}))
```

### RequestBodyActionLogFilter
This enrichment captures the arguments/ payload passed to an action method in an ASP.NET API endpoint. These arguments include query parameters, route values, form data, and complex objects (e.g., JSON payloads) passed in the request body.

Usage: By logging this data, you gain visibility into what input your API actions are receiving, allowing for better debugging, validation, and performance tuning. This also helps in diagnosing input-related errors or unexpected behavior when specific argument values are provided.

Register it like so:

```csharp
using Audacia.Log.AspNetCore;

public IConfiguration Configuration { get; set; }

public void ConfigureServices(IServiceCollection services)
{
  services.ConfigureActionContentLogging(Configuration);
  services.AddActionRequestBodyTelemetry();
  services.AddControllers(x => x.Filters.Add<LogRequestBodyActionFilterAttribute>());
}
```

### ResponseBodyActionLogFilter
This enrichment captures the response body sent back from the API or function. It includes the full content returned to the client, which can be valuable for tracing issues related to responses, errors, or unexpected results.

Usage: The response body data is useful for understanding what your API or function returned to a user or system, enabling the monitoring of output correctness and performance issues.

Register it like so:

```csharp
using Audacia.Log.AspNetCore;

public IConfiguration Configuration { get; set; }

public void ConfigureServices(IServiceCollection services)
{
  services.ConfigureActionContentLogging(Configuration);
  services.AddActionResponseBodyTelemetry();
  services.AddControllers(x => x.Filters.Add<LogResponseBodyActionFilterAttribute>());
}
```

### HttpDependencyBodyCaptureTelemetryInitializer
To add enrichment fo the request body and response body of dependency HTTP requests.

```csharp
using Audacia.Log.AspNetCore;

public IConfiguration Configuration { get; set; }

public void ConfigureServices(IServiceCollection services)
{
  services.ConfigureDependencyBodyContentLogging(Configuration);
  services.AddDependencyBodyTelemetry();
}
```

#### Configure "sub" and "role" override

To configure the overrides for "sub" and "role" add the `LogActionFilter` section to your `appsettings.json` file like below.

```json
{
 "LogActionFilter": {
  "IdClaimType": "oid",
  "RoleClaimType": "access"
 }
}
```

#### Configure global request and response filtering for controller actions
To globally configure the logging of actions you must add the `LogActionFilter` section to your `appsettings.json` file or equivalent.
This will allow the redaction of specific parameters from the request and response body during action logs.
This is case insensitive and will filter out parameters that contain the provided words.
For example using "Password" as the value will filter; Password, password, NewPassword, ConfirmPassword, etc.

```json
{
 "LogActionFilter": {
  "DisableBody": false,
  "MaxDepth":  10,
  "ExcludeArguments": [ "password", "token", "apikey" ],
  "IncludeClaims": [ "client_id" ]
 }
}
```

#### Configure global request and response body capture for dependency metrics
To globally configure the logging of dependencies you must add the `LogDependencyFilter` section to your `appsettings.json` file or equivalent.
This will allow for the refaction of specific parameters from the request and response body during dependency logs.
This is case insensitive and will filter out parameters that contain the provided words.
For example using "phonenumber" as the value will filter: PhoneNumber, MobilePhoneNumber, AlternativePhoneNumber, etc.

```json
{
 "LogDependencyFilter": {
    "DisableHttpTracking": false,
    "DisableHttpRequestBody": false,
    "DisableHttpResponseBody": false,
    "MaxDepth":  10,
    "ExcludeArguments": [ "password", "token", "apikey", "secret", "access_token", "refresh_token", "credential" ]
 }
}
```

#### Filtering specific requests
To filter specific requests the `LogFilterAttribute` can be used. 
To prevent the body content of the web request from being recorded use the `DisableBodyContent` parameter.
```c#
using Audacia.Log.AspNetCore;
...
[LogFilter(DisableBodyContent = true)]
public IActionRequest SetPassword(...)
{
    ...
}
```

If only a specific parameter needs to be excluded then that can also be done via the `ExcludeArguments` parameter. 
This is case insensitive and will filter out parameters that contain the provided words.
For example using "Password" as the value will filter; Password, password, NewPassword, ConfirmPassword, etc.
```c#
using Audacia.Log.AspNetCore;
...
[LogFilter(ExcludeArguments = new[] { "password" })]
public IActionRequest SalesforceLogin(string username, string password)
{
    ...
}
```

To limit the depth of the request that is logged you may use the `MaxDepth` parameter.
This is intended to prevent attacks on APIs that allow for dynamic request depths.
The default MaxDepth is 32.
```c#
using Audacia.Log.AspNetCore;
...
[LogFilter(MaxDepth = 3)]
...
```
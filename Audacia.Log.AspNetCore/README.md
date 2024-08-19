## Audacia.Log.AspNetCore

Standardized logging configuration for Audacia ASP.NET Core Web projects using Application Insights.

This is a standalone library (from v2.0.0 onwards) and is not dependent on Audacia.Log or Serilog. Please remove them from your web application when upgrading as going forwards the preferred approach is to use purely Application Insights and Microsoft logging abstractions.

### Usage

Copy the following json into your `appsettings.json` file. Configure the Instrumentation Key and  the QuickPulseApi Key to enable application insights for your app service.

 - For more information on Application Insights see, https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core
 - For more information on QuickPulse Metrics see, https://docs.microsoft.com/en-us/azure/azure-monitor/app/live-stream#secure-the-control-channel

The default logging providers are Debug, Console, EventLog, EventSource, ApplicationInsights. LogLevel on its own applies to all providers. Log levels can be altered by specifying the assembly name and log level for a logging provider.

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "00000000-0000-0000-0000-000000000000",
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

### Optional

#### ClaimsActionLogFilter
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

#### Add optional CustomAdditionalClaimsTelemetryProvider
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

#### RequestBodyActionLogFilter
This filter can be registered to include logs for the beginning and end of each Action Method. This only includes request parameters as well as details of the response such as the type of model returned. Register it like so:

```csharp
using Audacia.Log.AspNetCore;

public IConfiguration Configuration { get; set; }

public void ConfigureServices(IServiceCollection services)
{
  services.ConfigureActionContentLogging(Configuration);
  services.AddActionRequestBodyTelemetry(Configuration);
  services.AddControllers(x => x.Filters.Add<LogRequestBodyActionFilterAttribute>());
}
```

#### ResponseBodyActionLogFilter
This filter can be registered to include logs for the beginning and end of each Action Method. This only includes response body as well as details of the response such as the type of model returned. Register it like so:

```csharp
using Audacia.Log.AspNetCore;

public IConfiguration Configuration { get; set; }

public void ConfigureServices(IServiceCollection services)
{
  services.ConfigureActionContentLogging(Configuration);
  services.AddActionResponseBodyTelemetry(Configuration);
  services.AddControllers(x => x.Filters.Add<LogResponseBodyActionFilterAttribute>());
}
```

#### HttpDependencyBodyCaptureTelemetryInitializer
To add enrichment fo the request body and response body of dependency HTTP requests.

```csharp
using Audacia.Log.AspNetCore;

public IConfiguration Configuration { get; set; }

public void ConfigureServices(IServiceCollection services)
{
  services.ConfigureDependencyBodyContentLogging(Configuration);
  services.AddDependencyBodyTelemetry(Configuration);
}
```

##### Configure "sub" and "role" override

To configure the overrides for "sub" and "role" add the `LogActionFilter` section to your `appsettings.json` file like below.

```json
{
 "LogActionFilter": {
  "IdClaimType": "oid",
  "RoleClaimType": "access"
 }
}
```

##### Configure global request and response filtering
To globally configure the logging of actions you must add the `LogActionFilter` section to your `appsettings.json` file.
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

##### Filtering specific requests
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
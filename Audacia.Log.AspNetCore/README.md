## Audacia.Log.AspNetCore

Standardized logging configuration for Audacia ASP.NET Core Web projects using Application Insights.

### Usage

Copy the following json into youre `appsettings.json` file. Configure the Instrumentation Key and  the QuickPulseApi Key to enable application insights for your app service.

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

#### ActionLogFilter
This filter can be registered to include logs for the beginning and end of each Action Method. The request parameters are included, as well as details of the response such as the type of model returned. Register it like so:

```csharp
using Audacia.Log.AspNetCore;

public IConfiguration Configuration { get; set; }

public void ConfigureServices(IServiceCollection services)
{
	services.ConfigureActionContentLogging(Configuration);
	services.AddControllers(x => x.Filters.Add<LogActionFilterAttribute>());
}
```

##### Configure global request filtering
To globally configure the logging of actions you must add the `LogActionFilter` section to your `appsettings.json` file.
This will allow the redaction of specific parameters from the request body during action logs.
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

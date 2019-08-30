## Audacia.Log

Standardized logging configuration for Audacia projects using [Serilog](https://serilog.net).
If you're adding this to a ASP.NET Core web project, Then follow [these instructions](https://dev.azure.com/audacia/Audacia/_git/Audacia.Log?path=%2FAudacia.Log.AspNetCore&_a=readme) after finishing the base configuration detailed below.

### Usage

This package provides extension methods for adding the standard set of enrichers and sinks to a Serilog logger:

```c#

// This is the configuration object used to specify settings for the logger.
var config = new LogConfig
{
    ApplicationName = "Example App"; // The name of the application domain.
    EnvironmentName = "Development"; // The name of the environment the application is currently running in.
    IsDevelopment = false; // Specify whether or not this is a development environment, in which only trace sinks are used, and application insights output is sent to a local loopback.
    ApplicationInsightsKey = "00000000-0000-0000-0000-000000000000"; // The instrumentation key of an application insights resource. This is ignored if its null.
    SlackUrl = "[Slack Webhook]"; // The URL of a slack webhook to send error-level messages to. This is ignored if its null.
}
```

Set up the Serilog logger with all the default settings. This should be done in the main entry point of your app:

```c#
public static void Main(string[] args)
{
    Log.Logger = new LoggerConfiguration().ConfigureDefaults(config);
}
```

Alternatively, we can individually set minimum levels, enrichers, and sinks.

```c#
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Defaults() // Sets the default log levels (including filtering out noise from Microsoft and IdentityServer4 modules).
    .Enrich.WithDefaults(config) // add the default enrichers.
    .WriteTo.Defaults(config) // add the default sinks.
    .CreateLogger();
```

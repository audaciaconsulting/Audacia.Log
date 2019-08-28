## Audacia.Log

Standardized logging configuration for Audacia projects using Serilog.

### Usage

This package provides extension methods for adding the standard set of enrichers and sinks to a Serilog logger:

```c#

// This is the configuration object used to specify settings for the logger.
var config = new LogConfig
{
    ApplicationName = "
    EnvironmentName = "Development"; // The name of the environment the application is currently running in.
    IsDevelopment = false; // Specify whether or not this is a development environment, in which only trace sinks are used, and application insights output is sent to a local loopback.
    ApplicationInsightsKey = "00000000-0000-0000-0000-000000000000"; // The instrumentation key of an application insights resource.
    SlackUrl = "[Slack Webhook]"; // The URL of a slack webhook to send error-level messages to.
}
```

Set up the Serilog logger with all the default settings:

```c#
Log.Logger = new LoggerConfiguration().ConfigureDefaults(config);
```

Alternatively, we can individually set minimum levels, enrichers, and sinks.

```c#
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Defaults() // Sets the default log levels (including filtering out noise from Microsoft and IdentityServer4 modules).
    .Enrich.WithDefaults(config) // add the default enrichers.
    .WriteTo.Defaults(config) // add the default sinks.
    .CreateLogger();
```

In this example the arguments provided are as follows:

`environmentName`: This should be an identifier for the environment that the logs are being sent from. 

`isDevelopment`: Specifies whether the application is running in a development environment. If it is, application insights logs get sent to a local loopback and slack messages are suppressed.

`appInsightsKey`: This is the telemetry key for the application insights azure resource.

`slackUrl`: This is a URL for a slack webhook to which to send messages. Only error-level logs and above are sent.


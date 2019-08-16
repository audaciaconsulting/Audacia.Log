## Audacia.Log

Standardized logging configuration for Audacia projects using Serilog.

### Usage

This package provides extension methods for adding the standard set of enrichers and sinks to a Serilog logger:

```c#
var environmentName = "development";
var appInsightsKey = "00000000-0000-0000-0000-000000000000";
var slackUrl = "[Slack Webhook]";
var isDevelopment = true;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithDefaults(environment) // add the default enrichers
    .WriteTo.Defaults(isDevelopment, appInsightsKey, slackUrl) // add the default sinks
    .CreateLogger();

```

In this example the arguments provided are as follows:

`environmentName`: This should be an identifier for the environment that the logs are being sent from. 

`isDevelopment`: Specifies whether the application is running in a development environment. If it is, application insights logs get sent to a local loopback and slack messages are suppressed.

`appInsightsKey`: This is the telemetry key for the application insights azure resource.

`slackUrl`: This is a URL for a slack webhook to which to send messages. Only error-level logs and above are sent.
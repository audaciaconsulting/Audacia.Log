﻿## Audacia.Log

Standardized logging configuration for Audacia projects using [Serilog](https://serilog.net).
This package should only be added if the application is a legacy, if ASP.NET Core use [Audacia.Log.AspNetCore](https://github.com/audaciaconsulting/Audacia.Log) instead.

### Usage

This package provides extension methods for adding the standard set of enrichers and sinks to a Serilog logger:

```c#

// This is the configuration object used to specify settings for the logger.
var config = new AudaciaLoggerConfiguration
{
    ApplicationName = "Example App"; // The name of the application domain.
    EnvironmentName = "Development"; // The name of the environment the application is currently running in.
    ApplicationInsightsKey = "00000000-0000-0000-0000-000000000000"; // The instrumentation key of an application insights resource. This is ignored if its null.
}
```

Set up the Serilog logger with all the default settings. This should be done in the main entry point of your app:

```c#
public static void Main(string[] args)
{
    Log.Logger = new LoggerConfiguration().ConfigureDefaults(config).CreateLogger();
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

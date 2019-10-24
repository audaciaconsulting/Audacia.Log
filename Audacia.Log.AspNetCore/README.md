## Audacia.Log.AspNetCore

Standardized logging configuration for Audacia ASP.NET Core Web projects using [Serilog](https://serilog.net).

### Usage

After following the instructions [here](https://dev.azure.com/audacia/Audacia/_git/Audacia.Log?path=%2FREADME.md) you will have Serilog configured and can start integrating it into the ASP.NET pipeline.

Settings can also be specified programatically using the `LogConfig` type, however, the easiest way to specify logging settings is using the following entry in your `appsettings.json` file:

```json
"Logging": {
	"ApplicationName": "ExampleApp",
	"EnvironmentName": "User Acceptance",
	"IsDevelopment": false,
	"ApplicationInsightsKey": "00000000-0000-0000-0000-000000000000",
	"SlackUrl": "[Slack Webhook]"
},
```

After configuring the settings for each environment, the logging needs to be applied to the Web Host Builder. Settings can be provided via a `LogConfig` object, or you can not provide one and settings will be read from your cofiguration file:

```c#
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
	WebHost.CreateDefaultBuilder(args)
		.UseKestrel(c => c.AddServerHeader = false)
		.ConfigureLogging() // Add this line to read settings from your appsettings.json file.
		.UseStartup<Startup>();
```

There is a similar extension method provided for configuring the logging service in your `Startup.cs`. Similarly, a `LogConfig` can be passed to this method if you don't want to specify settings in your configuration file.a
It is recommended to call this method first in the chain so any subsequent errors that may occur are logged correctly.

```c#
public void ConfigureServices(IServiceCollection services)
{
	services.ConfigureLogging();
}
```

### Optional

#### ActionLogFilter
This filter can be registered to include logs for the beginning and end of each Action Method. The request parameters are included, as well as details of the response such as the type of model returned. Register it like so:

```c#
serviceCollection.AddMvcCore(x => x.Filters.Add<ActionLogFilter>())
```

#### HttpLogMiddleware
This middleware can be used to log every HTTP request and response, with details of each included in the log context. **Its not recommended to use this with Application Insights because Application Insights has its own HTTP logging**.
It can be registered in `Startup.cs` as follows:

```c#
public void Configure(IApplicationBuilder app)
{
	app.UseMiddleware<HttpLogMiddleware>();
}
```
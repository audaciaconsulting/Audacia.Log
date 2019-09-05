﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;

namespace Audacia.Log.AspNetCore
{
    /// <summary>Middleware for logging the details of each HTTP request and response.</summary>
	public class HttpLogMiddleware
    {
        private const string RequestingTemplate = "Requesting HTTP {Method} to '{Path}'";
        private const string RespondingTemplate = "Responding HTTP {Method} to '{Path}' with {StatusCode} in {Elapsed:0.0000} ms";


        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        /// <summary>Creates a new instance of <see cref="HttpLogMiddleware"/>.</summary>
        public HttpLogMiddleware(RequestDelegate next, ILogger logger)
        {
            var requestDelegate = next;
            _next = requestDelegate ?? throw new ArgumentNullException(nameof(next));
            _logger = logger?.ForContext<HttpLogMiddleware>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Invokes this middleware.</summary>
        /// <exception cref="ArgumentNullException">The <param name="httpContext"></param> argument is null.</exception>
        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                var request = new
                {
                    Headers = httpContext.Request.Headers?.Where(h => h.Key != "Authorization").Select(x => x.Key + ": " + x.Value),
                    httpContext.Request.Host.Value,
                    httpContext.Request.Method,
                    Path = httpContext.Request.Path.Value,
                    httpContext.Request.Protocol,
                    QueryString = httpContext.Request.QueryString.ToString(),
                    httpContext.Request.Scheme,
                    httpContext.Request.ContentLength,
                    httpContext.Request.ContentType,
                    httpContext.Request.HasFormContentType
                };

                var connection = new
                {
                    httpContext.Connection.Id,
                    ClientCertificate = httpContext.Connection.ClientCertificate?.FriendlyName,
                    httpContext.Connection.LocalPort,
                    httpContext.Connection.RemotePort,
                    LocalIpAddress = httpContext.Connection.LocalIpAddress.ToString(),
                    RemoteIpAddress = httpContext.Connection.RemoteIpAddress.ToString(),
                };

                var requestLog = _logger
                    .ForContext("Request", request, true)
                    .ForContext("Connection", connection, true);


                requestLog.Write(LogEventLevel.Information, RequestingTemplate, httpContext.Request.Method, httpContext.Request.Path);

                await _next(httpContext);

                var statusCode = httpContext.Response?.StatusCode ?? 0;
                var level = statusCode > 499 ? LogEventLevel.Error
                    : statusCode > 366 ? LogEventLevel.Warning 
                    : LogEventLevel.Information;

                var response = httpContext.Response == null ? null : new
                {
                    Headers = httpContext.Response.Headers?.Select(x => x.Key + ": " + x.Value),
                    httpContext.Response.ContentLength,
                    httpContext.Response.ContentType
                };

                var responseLog = requestLog.ForContext("Response", response, true);

                responseLog.Write(level,
                    RespondingTemplate,
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    statusCode,
                    stopwatch.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, RespondingTemplate,
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    500,
                    stopwatch.Elapsed.TotalMilliseconds);
                throw;
            }
        }
    }
}
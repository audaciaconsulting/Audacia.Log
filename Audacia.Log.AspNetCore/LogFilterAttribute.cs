using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Allows the configuration of the <see cref="LogClaimsActionFilterAttribute "/> and <see cref="LogRequestBodyActionFilterAttribute "/> per request.
/// </summary>
/// <example>
/// To use place above a controller action and specify one or more parameters.
/// [LogFilter(DisableBodyContent = true)]
/// [LogFilter(ExcludeArguments = new[] { "password" })]
/// [LogFilter(IncludeClaims = new[] { "user.search" })].
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class LogFilterAttribute : Attribute, IFilterMetadata
{
    /// <summary>
    /// Gets or sets the names of arguments to exclude from the logs.
    /// </summary>
    public string[]? ExcludeArguments { get; set; }

    /// <summary>
    /// Gets or sets the names of claims to include in the logs.
    /// </summary>
    public string[]? IncludeClaims { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the request body data should be logged.
    /// </summary>
    public bool DisableBodyContent { get; set; }

    /// <summary>
    /// Gets or sets the max depth for deconstructing objects in the request body.
    /// </summary>
    public int MaxDepth { get; set; }
}
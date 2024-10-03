using System.Collections.Generic;

namespace Audacia.Log.AspNetCore.Configuration;

/// <summary>
/// Allows the configuration of the capturing and logging of dependency requests.
/// </summary>
public sealed class LogDependencyFilterConfig
{
    /// <summary>
    /// Gets or sets the names of arguments to exclude from the logs.
    /// </summary>
    public IReadOnlyCollection<string> ExcludeArguments { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets a value indicating whether the HTTP dependencies should be captured and logged.
    /// </summary>
    public bool DisableHttpTracking { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to log out the request body from HTTP dependency requests.
    /// </summary>
    public bool DisableHttpRequestBody { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to log out the response body from HTTP dependency requests.
    /// </summary>
    public bool DisableHttpResponseBody { get; set; }

    /// <summary>
    /// Gets or sets the max depth for deconstructing objects in the request body.
    /// </summary>
    public int MaxDepth { get; set; }
}
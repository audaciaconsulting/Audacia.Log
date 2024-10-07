using System.Collections.Generic;

namespace Audacia.Log.AspNetCore.Configuration;

/// <summary>
/// Allows the configuration of the <see cref="LogClaimsActionFilterAttribute "/> and <see cref="LogRequestBodyActionFilterAttribute "/> per request.
/// </summary>
public sealed class LogActionFilterConfig
{
    /// <summary>
    /// Gets the location of where the config should be located.
    /// </summary>
    public const string Location = "LogActionFilter";

    /// <summary>
    /// Gets or sets the names of arguments to exclude from the logs.
    /// </summary>
    public IReadOnlyCollection<string> ExcludeArguments { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the names of claims to include in the logs.
    /// </summary>
    public IReadOnlyCollection<string> IncludeClaims { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets a value indicating whether the request body data should be logged.
    /// </summary>
    public bool DisableBodyContent { get; set; }

    /// <summary>
    /// Gets or sets the max depth for deconstructing objects in the request body.
    /// </summary>
    public int MaxDepth { get; set; }

    /// <summary>
    /// Gets or sets id claim name.
    /// </summary>
    public string IdClaimType { get; set; } = ClaimTypes.Sub;

    /// <summary>
    /// Gets or sets role claim name.
    /// </summary>
    public string RoleClaimType { get; set; } = ClaimTypes.Role;
}
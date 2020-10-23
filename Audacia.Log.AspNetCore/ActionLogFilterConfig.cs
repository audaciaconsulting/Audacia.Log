namespace Audacia.Log.AspNetCore
{
    /// <summary>
    /// Allows the configuration of the <see cref="ActionLogFilterAttribute"/> per request.
    /// </summary>
    public sealed class ActionLogFilterConfig
    {
        /// <summary>
        /// Gets or sets the names of arguments to exclude from the logs.
        /// </summary>
        public string[] ExcludeArguments { get; set; }

        /// <summary>
        /// Gets or sets the names of claims to include in the logs.
        /// </summary>
        public string[] IncludeClaims { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request body data should be logged.
        /// </summary>
        public bool DisableBodyContent { get; set; }

        /// <summary>
        /// Gets or sets the max depth for desconstructing objects in the request body.
        /// </summary>
        public int MaxDepth { get; set; }
    }
}
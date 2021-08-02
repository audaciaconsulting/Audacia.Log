namespace Audacia.Log.AspNetCore
{
    /// <summary>
    /// Provides access to <see cref="ActionLogFilterConfig"/> on the action and/or globally.
    /// </summary>
    public interface IActionLogFilterConfigAccessor
    {
        /// <summary>
        /// Gets the global configuration for logging action content.
        /// </summary>
        ActionLogFilterConfig GlobalConfiguration { get; }

        /// <summary>
        /// Gets or sets the action specific configuration for logging action content.
        /// </summary>
        ActionLogFilterConfig ActionConfiguration { get; set; }
    }
}

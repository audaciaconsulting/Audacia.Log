using Microsoft.Extensions.Options;

namespace Audacia.Log.AspNetCore
{
    /// <summary>
    /// Provides access to <see cref="ActionLogFilterConfig"/> on the action and/or globally.
    /// </summary>
    public sealed class ActionLogFilterConfigAccessor : IActionLogFilterConfigAccessor
    {
        /// <summary>
        /// Gets the global configuration for logging action content.
        /// </summary>
        public ActionLogFilterConfig GlobalConfiguration { get; }

        /// <summary>
        /// Gets or sets the action specific configuration for logging action content.
        /// </summary>
        public ActionLogFilterConfig ActionConfiguration { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="ActionLogFilterConfigAccessor"/>.
        /// </summary>
        /// <param name="options">Global configuration options</param>
        public ActionLogFilterConfigAccessor(IOptions<ActionLogFilterConfig> options)
        {
            GlobalConfiguration = options?.Value;
            ActionConfiguration = null;
        }
    }
}

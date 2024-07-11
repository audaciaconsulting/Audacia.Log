using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Gets additional claims for TelemetryInitialisers.
/// </summary>
public interface IAdditionalClaimsTelemetryProvider
{
    /// <summary>
    /// Gets additional claims.
    /// </summary>
    /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
    /// <returns>List of Tuples(Name,Data).</returns>
    List<(string Name, string Data)> GetClaims(IHttpContextAccessor httpContextAccessor);
}

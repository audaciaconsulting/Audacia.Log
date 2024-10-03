using System.Collections.Generic;
using System.Security.Claims;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Gets additional claims for Telemetry Initialisers.
/// </summary>
public interface IAdditionalClaimsTelemetryProvider
{
    /// <summary>
    /// Gets additional claims.
    /// </summary>
    /// <param name="claims">User claims <see cref="Claim"/> collection.</param>
    /// <returns>List of Tuples(Name,Data).</returns>
    List<ClaimsData> GetClaims(IEnumerable<Claim> claims);
}
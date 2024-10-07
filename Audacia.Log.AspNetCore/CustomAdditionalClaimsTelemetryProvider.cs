using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Implements <see cref="IAdditionalClaimsTelemetryProvider"/> to get additional claims.
/// </summary>
public class CustomAdditionalClaimsTelemetryProvider : IAdditionalClaimsTelemetryProvider
{
    private readonly Func<IEnumerable<Claim>, List<ClaimsData>> _claimsGetter;

    /// <summary>
    /// Initialise the claimsGetter Func.
    /// </summary>
    /// <param name="claimsGetter">Claims getters.</param>
    public CustomAdditionalClaimsTelemetryProvider(Func<IEnumerable<Claim>, List<ClaimsData>> claimsGetter)
    {
        _claimsGetter = claimsGetter;
    }

    /// <summary>
    /// Gets additional claims.
    /// </summary>
    /// <param name="claims">User claims <see cref="Claim"/> collection.</param>
    /// <returns>List of <see cref="ClaimsData"/>.</returns>
    public List<ClaimsData> GetClaims(IEnumerable<Claim> claims)
    {
        return _claimsGetter(claims);
    }
}
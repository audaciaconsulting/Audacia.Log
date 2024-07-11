using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Implements <see cref="IAdditionalClaimsTelemetryProvider"/> to get additional claims.
/// </summary>
public class CustomAdditionalClaimsTelemetryProvider : IAdditionalClaimsTelemetryProvider
{
    private readonly Func<ClaimsPrincipal, List<(string, string)>> _claimsGetter;

    /// <summary>
    /// Initialise the claimsGetter Func.
    /// </summary>
    /// <param name="claimsGetter"></param>
    public CustomAdditionalClaimsTelemetryProvider(Func<ClaimsPrincipal, List<(string Name, string Data)>> claimsGetter)
    {
        _claimsGetter = claimsGetter;
    }

    /// <summary>
    /// Gets additional claims.
    /// </summary>
    /// <param name="httpContextAccessor"><see cref="IHttpContextAccessor"/>.</param>
    /// <returns>List of Tuples(Name,Data).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public List<(string Name, string Data)> GetClaims(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor == null || httpContextAccessor.HttpContext == null) 
        {
            throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        return _claimsGetter(httpContextAccessor.HttpContext.User);
    }
}

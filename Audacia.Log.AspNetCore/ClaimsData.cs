namespace Audacia.Log.AspNetCore;

/// <summary>
/// ClaimsData record to hold type and value of a selected user claim.
/// </summary>
public class ClaimsData 
{
    /// <summary>
    /// Gets or sets Type of the claim.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets value of the claim.
    /// </summary>
    public string Data { get; set; } = string.Empty;
}

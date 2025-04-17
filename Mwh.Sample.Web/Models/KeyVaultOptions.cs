namespace Mwh.Sample.Web.Models;
/// <summary>
/// Configuration options for Azure Key Vault integration
/// </summary>
public class KeyVaultOptions
{
    /// <summary>
    /// Gets or sets the authentication mode for Key Vault access
    /// </summary>
    public KeyVaultUsage Mode { get; set; }

    /// <summary>
    /// Gets or sets the URI of the Azure Key Vault instance
    /// </summary>
    public string? KeyVaultUri { get; set; }

    /// <summary>
    /// Gets or sets the client ID for service principal authentication
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret for service principal authentication
    /// </summary>
    public string? ClientSecret { get; set; }
}

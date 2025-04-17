namespace Mwh.Sample.Web.Models;
/// <summary>
/// Defines authentication modes for Azure Key Vault access
/// </summary>
public enum KeyVaultUsage
{
    /// <summary>
    /// Use local secret store for development environments
    /// </summary>
    UseLocalSecretStore,

    /// <summary>
    /// Authenticate using client ID and secret credentials
    /// </summary>
    UseClientSecret,

    /// <summary>
    /// Authenticate using Managed Service Identity
    /// </summary>
    UseMsi
}

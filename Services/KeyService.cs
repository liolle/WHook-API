using System.Security.Cryptography;

namespace whook.services;

public class KeyService
{
  public string GenerateApiKey()
  {
    // Generate a cryptographically secure API key
    using RandomNumberGenerator rng = RandomNumberGenerator.Create();
    byte[] tokenData = new byte[32]; // 256 bits
    rng.GetBytes(tokenData);

    // Convert to Base64Url string (URL-safe)
    string base64 = Convert.ToBase64String(tokenData);
    return base64.Replace('+', '-')
      .Replace('/', '_')
      .Replace("=", "");
  }
}

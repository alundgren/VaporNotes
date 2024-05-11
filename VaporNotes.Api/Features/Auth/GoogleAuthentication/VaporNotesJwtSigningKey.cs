using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using VaporNotes.Api.Support;

namespace VaporNotes.Api.Features.Auth.GoogleAuthentication;

public class VaporNotesJwtSigningKey(IConfiguration configuration, ILogger<VaporNotesJwtSigningKey> logger)
{
    public static string GenerateSymmetricKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)); // 512 bits for HMACSHA512;
    private SymmetricSecurityKey? key = null;
    private static object keyLock = new object();

    public SymmetricSecurityKey GetKey()
    {
        if (key != null)
            return key;

        SymmetricSecurityKey localKey;
        lock(keyLock)
        {
            string? keyData = configuration["VaporNotes:JwtSigningKey"];
            if(keyData != null)
                Console.WriteLine("Using signing key from settings");
            else
            {
                keyData = GenerateSymmetricKey();
                logger.LogInformation($"New signing key generated: {keyData}");
            }

            localKey = FromString(keyData);
            key = localKey;
        }
        return localKey;
    }
    private SymmetricSecurityKey FromString(string storedKey) => new SymmetricSecurityKey(Convert.FromBase64String(storedKey));

    public static string SymmetricKeySignatureAlgorithmName = SecurityAlgorithms.HmacSha512;
}

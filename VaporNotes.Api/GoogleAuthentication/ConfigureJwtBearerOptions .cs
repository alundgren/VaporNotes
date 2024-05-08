using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace VaporNotes.Api.GoogleAuthentication;

// This class is needed to inject the JWT validation options including the public key
public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string name, JwtBearerOptions options)
    {
        RSA rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(JwtGenerator.PublicKey), out _);

        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa),
            ValidateIssuer = true,
            ValidIssuer = "VaporNotesApiAuthService",
            ValidateAudience = true,
            ValidAudience = "VaporNotesApi",
            CryptoProviderFactory = new CryptoProviderFactory()
            {
                CacheSignatureProviders = false
            }
        };
    }

    public void Configure(JwtBearerOptions options)
    {
        throw new NotImplementedException();
    }
}

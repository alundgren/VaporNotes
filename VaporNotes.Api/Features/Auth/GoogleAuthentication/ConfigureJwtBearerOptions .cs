using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace VaporNotes.Api.Features.Auth.GoogleAuthentication;

// This class is needed to inject the JWT validation options including the public key
public class ConfigureJwtBearerOptions(VaporNotesJwtSigningKey signingKey) : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string? name, JwtBearerOptions options) => Configure(options);

    public void Configure(JwtBearerOptions options)
    {
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey.GetKey(),
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
}

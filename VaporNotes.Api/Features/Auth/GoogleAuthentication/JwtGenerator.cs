using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace VaporNotes.Api.Features.Auth.GoogleAuthentication;

public class JwtGenerator(VaporNotesJwtSigningKey signingKey)
{
    public string CreateUserAccessToken(string userId, DateTimeOffset expirationDate)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = "VaporNotesApi",
            Issuer = "VaporNotesApiAuthService",
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Sid, userId.ToString())
            }),
            Expires = expirationDate.UtcDateTime,
            SigningCredentials = new SigningCredentials(signingKey.GetKey(), VaporNotesJwtSigningKey.SymmetricKeySignatureAlgorithmName)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

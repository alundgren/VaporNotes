using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace VaporNotes.Api.Features.Auth.GoogleAuthentication;

public class JwtGenerator(VaporNotesJwtSigningKey signingKey)
{
    public string CreateUserAuthToken(string userId)
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
            Expires = DateTime.UtcNow.AddMinutes(60),
            SigningCredentials = new SigningCredentials(signingKey.GetKey(), VaporNotesJwtSigningKey.SymmetricKeySignatureAlgorithmName)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

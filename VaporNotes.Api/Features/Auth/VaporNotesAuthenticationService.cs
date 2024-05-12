using Google.Apis.Auth;
using System.ComponentModel.DataAnnotations;
using VaporNotes.Api.Features.Auth.GoogleAuthentication;
using VaporNotes.Api.Support;

namespace VaporNotes.Api.Features.Auth;

/// <summary>
/// The uses logis in to the web app using google and then passes the id token to us. 
/// We verify the identity and issue the user and access token.
/// </summary>
public class VaporNotesAuthenticationService(VaporNotesJwtSigningKey signingKey, IConfiguration configuration)
{
    public async Task<VaporNotesAccessToken> AuthenticateAsync(AuthenticateRequest request)
    {
        var generator = new JwtGenerator(signingKey);

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new List<string>() { configuration.GetRequiredSettingValue("VaporNotes:GoogleClientId") }
        };

        GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        var expirationDate = DateTimeOffset.Now.AddHours(8);
        var accessToken = generator.CreateUserAccessToken(payload.Subject, DateTimeOffset.Now.AddHours(8));
        return new VaporNotesAccessToken(accessToken, expirationDate);
    }
}

public record AuthenticateRequest([Required]string IdToken);

//TODO: Include a refresh token?
public record VaporNotesAccessToken(string AccessToken, DateTimeOffset ExpirationDate);
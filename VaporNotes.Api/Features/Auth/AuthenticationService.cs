using System.ComponentModel.DataAnnotations;

namespace VaporNotes.Api.Features.Auth;

/// <summary>
/// The uses logis in to the web app using google and then passes the id token to us. 
/// We verify the identity and issue the user and access token.
/// </summary>
public class AuthenticationService()
{
    public async Task<VaporNotesAccessToken> AuthenticateAsync(AuthenticateRequest request)
    {
        throw new NotImplementedException();
    }
}

public record AuthenticateRequest([Required]string IdToken);

//TODO: Include a refresh token?
public record VaporNotesAccessToken(string AccessToken, DateTimeOffset ExpirationDate);
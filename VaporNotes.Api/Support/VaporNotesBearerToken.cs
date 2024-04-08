namespace VaporNotes.Api.Support;

public class VaporNotesBearerToken(IHttpContextAccessor httpContextAccessor)
{
    public string? AccessToken
    {
        get 
        {
            string? auth = ((string?)httpContextAccessor?.HttpContext?.Request?.Headers?["Authorization"])?.Trim();
            if (auth == null)
                return null;
            const string TokenType = "Bearer ";
            if (!auth.StartsWith(TokenType, StringComparison.OrdinalIgnoreCase))
                return null;
            return auth.Substring(TokenType.Length);
        }
    }

    public string RequiredAccessToken
    {
        get
        {
            var token = AccessToken;
            if (token == null)
                throw new Exception("Missing access token");
            return token;
        }
    }
}

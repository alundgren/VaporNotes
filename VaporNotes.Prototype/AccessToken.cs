namespace VaporNotes.Prototype
{
    public record DropboxToken(string AccessToken, DateTimeOffset ExpirationDateUtc);
}

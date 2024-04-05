namespace VaporNotes.Api;
public record CompleteAuthorizeRequest(string Code); //TODO: Required
public record ListNotesRequest(string AccessToken);
public record AddTextNoteRequest(string AccessToken, string Text);
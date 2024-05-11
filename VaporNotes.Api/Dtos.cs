namespace VaporNotes.Api;
public record CompleteAuthorizeRequest(string Code); //TODO: Required
public record RefreshAuthorizeRequest(string RefreshToken); //TODO: Required
public record ListNotesRequest();
public record AddTextNoteRequest(string Text);
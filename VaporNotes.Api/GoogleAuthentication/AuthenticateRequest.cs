using System.ComponentModel.DataAnnotations;

namespace VaporNotes.Api.GoogleAuthentication;
public record AuthenticateRequest([Required] string IdToken);
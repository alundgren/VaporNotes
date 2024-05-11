using System.ComponentModel.DataAnnotations;

namespace VaporNotes.Api.Features.Auth.GoogleAuthentication;
public record AuthenticateRequest([Required] string IdToken);
namespace VaporNotes.Api.Core;

public static class LoggedBuiltinBuilderExtensions
{
    public static IApplicationBuilder UseAuthenticationLogged(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.Properties["__AuthenticationMiddlewareSet"] = true;
        return app.UseMiddleware<LoggedAuthenticationMiddleware>();
    }

    public static IApplicationBuilder UseAuthorizationLogged(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.Properties["__AuthorizationMiddlewareSet"] = true;
        return app.UseMiddleware<LoggedAuthorizationMiddleware>();
    }
}

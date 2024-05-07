using Microsoft.AspNetCore.Authentication;

namespace VaporNotes.Api.Core;

public class LoggedAuthenticationMiddleware
{
    private readonly AuthenticationMiddleware middleware;
    private readonly ILogger<LoggedAuthenticationMiddleware> logger;
    private readonly IWebHostEnvironment webHostEnvironment;

    public LoggedAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes, ILogger<LoggedAuthenticationMiddleware> logger, IWebHostEnvironment webHostEnvironment)
    {
        this.logger = logger;
        this.webHostEnvironment = webHostEnvironment;
        middleware = new AuthenticationMiddleware(next, schemes);
    }

    public IAuthenticationSchemeProvider Schemes
    {
        get
        {
            return middleware.Schemes;
        }
        set
        {
            middleware.Schemes = value;
        }
    }


    public async Task Invoke(HttpContext context)
    {
        if (webHostEnvironment.IsDevelopment())
        {
            string N(AuthenticationScheme? s) => s?.DisplayName ?? "-";
            var defaultScheme = await Schemes.GetDefaultAuthenticateSchemeAsync();
            var requestSchemes = (await Schemes.GetRequestHandlerSchemesAsync())?.Select(N) ?? Enumerable.Empty<string>();
            var endpoint = context.GetEndpoint();

            logger.Log(LogLevel.Information, $"AuthenticationMiddleware({endpoint?.DisplayName}); DefaultScheme: {N(defaultScheme)}, RequestSchemes: [{string.Join(", ", requestSchemes)}] User: {context.User?.Identity?.Name ?? "-"}");
        }
        await middleware.Invoke(context);
    }
}
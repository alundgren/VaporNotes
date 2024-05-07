using Microsoft.AspNetCore.Authorization;

namespace VaporNotes.Api.Core;

public class LoggedAuthorizationMiddleware
{
    private readonly AuthorizationMiddleware middleware;
    private readonly ILogger<LoggedAuthorizationMiddleware> logger;
    private readonly IWebHostEnvironment webHostEnvironment;
    private readonly IAuthorizationPolicyProvider policyProvider;

    public LoggedAuthorizationMiddleware(
        RequestDelegate next,
        IServiceProvider services,
        IAuthorizationPolicyProvider policyProvider,
        ILogger<AuthorizationMiddleware> innerLogger,
        ILogger<LoggedAuthorizationMiddleware> logger,
        IWebHostEnvironment webHostEnvironment)
    {
        middleware = new AuthorizationMiddleware(next, policyProvider, services, innerLogger);
        this.policyProvider = policyProvider;
        this.logger = logger;
        this.webHostEnvironment = webHostEnvironment;
    }

    public async Task Invoke(HttpContext context)
    {
        if (webHostEnvironment.IsDevelopment())
        {
            var endpoint = context.GetEndpoint();
            IReadOnlyList<IAuthorizeData> authorizeData = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>() ?? Array.Empty<IAuthorizeData>();
            IReadOnlyList<AuthorizationPolicy> policies = endpoint?.Metadata.GetOrderedMetadata<AuthorizationPolicy>() ?? Array.Empty<AuthorizationPolicy>();
            var policy = await AuthorizationPolicy.CombineAsync(policyProvider, authorizeData, policies);

            string Describe<T>(IReadOnlyList<T>? items, string name, Func<T, string> describeItem) =>
                $"{name}: [{string.Join(", ", (items ?? Array.Empty<T>()).Select(describeItem))}]";

            var schemes = Describe(policy?.AuthenticationSchemes, "AuthenticationSchemes", x => x);
            var requirements = Describe(policy?.Requirements, "Requirements", x => x?.ToString() ?? "-");
            logger.LogInformation($"AuthorizeMiddleware({endpoint?.DisplayName}); {schemes}; {requirements}");
        }
        await middleware.Invoke(context);
    }
}

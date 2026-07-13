namespace RalseiBot.Web.Components.Security;

/// <summary>
///     This handles the general cookie forwarding for the API.
/// </summary>
/// <param name="httpContextAccessor">The HTTP Context Accessor, so we can insert the Authorization Cookie.</param>
public class CookieForwardingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated != true) return base.SendAsync(request, cancellationToken);

        var tokenClaim = httpContext.User.FindFirst("RawJwtToken")?.Value;

        if (!string.IsNullOrEmpty(tokenClaim))
            request.Headers.Add("Cookie", $"X-Access-Token={tokenClaim}");

        return base.SendAsync(request, cancellationToken);
    }
}
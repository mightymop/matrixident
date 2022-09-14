using AspNetCoreRateLimit;

namespace MatrixIdent
{
    public class CustomClientResolveContributor : IClientResolveContributor
    {
        Task<string> IClientResolveContributor.ResolveClientAsync(HttpContext httpContext)
        {
            var authToken = string.Empty;
            if (httpContext.Request.Headers.TryGetValue("Authorization",out var values))
            {
                authToken = values.First().Split(" ")[1];
            }

            return Task.FromResult(authToken);
        }
    }
}

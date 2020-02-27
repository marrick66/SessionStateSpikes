using Microsoft.AspNetCore.Builder;
using SessionLib.AspNetCore;

namespace SessionLib.DependencyInjection
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSharedSessions(
            this IApplicationBuilder app)
        {
            //We still need the session middleware to run prior to ours,
            //so that the session object itself is available for modification.
            return app
                .UseSession()
                .UseMiddleware<SharedSessionMiddleware>();

        }
    }
}

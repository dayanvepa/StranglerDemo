using Microsoft.AspNetCore.Builder;

namespace StranglerProxy.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseJsonToXmlMiddleware(this IApplicationBuilder app, string pathPrefix = "/api/orders", bool addTransformHeader = true)
        {
            return app.UseMiddleware<JsonToXmlMiddleware>(pathPrefix, addTransformHeader);
        }
    }
}
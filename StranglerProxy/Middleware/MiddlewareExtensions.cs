using Microsoft.AspNetCore.Builder;

namespace StranglerProxy.Middleware
{
    /// <summary>
    /// Contiene métodos de extensión para el pipeline de middleware de ASP.NET Core.
    /// </summary>
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseJsonToXmlMiddleware(this IApplicationBuilder app, string pathPrefix = "/api/orders", bool addTransformHeader = true)
        {
            return app.UseMiddleware<JsonToXmlMiddleware>(pathPrefix, addTransformHeader);
        }
    }
}
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace StranglerProxy.Middleware
{
    public class JsonToXmlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _pathPrefix;
        private readonly bool _addTransformHeader;

        public JsonToXmlMiddleware(RequestDelegate next, string pathPrefix = "/api/orders", bool addTransformHeader = true)
        {
            _next = next;
            _pathPrefix = pathPrefix;
            _addTransformHeader = addTransformHeader;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Solo transformamos si la ruta coincide (ajusta según necesites)
            if (!context.Request.Path.StartsWithSegments(_pathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Guardamos el body original
            var originalBodyStream = context.Response.Body;

            await using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            // Ejecuta pipeline (incluye YARP) — la respuesta quedará en memStream
            await _next(context);

            // Leemos la respuesta backend
            memStream.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(memStream, Encoding.UTF8).ReadToEndAsync();

            var contentType = context.Response.ContentType ?? string.Empty;
            var statusCode = context.Response.StatusCode;

            // Si es JSON y status 2xx -> convertir
            if (statusCode >= 200 && statusCode < 300 &&
                contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var segment = _pathPrefix.Trim('/').Split('/').Last();
                    var rootName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(segment.ToLower());
                    var xml = Helpers.JsonXmlConverter.ConvertJsonToXml(responseText, rootName);

                    var xmlBytes = Encoding.UTF8.GetBytes(xml);

                    // Restauramos el body original y escribimos XML
                    context.Response.Body = originalBodyStream;
                    context.Response.ContentType = "application/xml; charset=utf-8";
                    context.Response.ContentLength = xmlBytes.Length;

                    if (_addTransformHeader)
                    {
                        context.Response.Headers["Transformed-By"] = "ACL-Proxy";
                    }

                    await context.Response.Body.WriteAsync(xmlBytes, 0, xmlBytes.Length);
                }
                catch (JsonException je)
                {
                    // Si la conversión falla, devolvemos la respuesta original JSON (fallback)
                    context.Response.Body = originalBodyStream;
                    memStream.Seek(0, SeekOrigin.Begin);
                    await memStream.CopyToAsync(originalBodyStream);
                    // Log en consola (puedes inyectar logger si deseas)
                    Console.Error.WriteLine($"JsonToXmlMiddleware - JsonException: {je}");
                }
                catch (System.Exception ex)
                {
                    context.Response.Body = originalBodyStream;
                    memStream.Seek(0, SeekOrigin.Begin);
                    await memStream.CopyToAsync(originalBodyStream);
                    Console.Error.WriteLine($"JsonToXmlMiddleware - Exception: {ex}");
                }
            }
            else
            {
                // No es JSON: devolver tal cual
                context.Response.Body = originalBodyStream;
                memStream.Seek(0, SeekOrigin.Begin);
                await memStream.CopyToAsync(originalBodyStream);
            }
        }
    }
}
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace StranglerProxy.Middleware
{
    /// <summary>
    /// Middleware que intercepta respuestas JSON de ciertas rutas y las convierte a XML antes de enviarlas al cliente.
    /// </summary>
    /// <remarks>
    /// Se usa típicamente en una aplicación proxy/strangler que consume servicios de backend que devuelven JSON,
    /// pero donde los consumidores esperan XML. El middleware mira únicamente respuestas con Content-Type
    /// "application/json" y código de estado 2xx en rutas que comienzan con <see cref="_pathPrefix"/>.
    /// </remarks>
    public class JsonToXmlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _pathPrefix;
        private readonly bool _addTransformHeader;

        /// <summary>
        /// Crea una nueva instancia del middleware.
        /// </summary>
        /// <param name="next">El siguiente delegado en el pipeline de ASP.NET Core.</param>
        /// <param name="pathPrefix">
        /// Prefijo de ruta que se debe comparar antes de intentar la transformación. Solo las rutas que
        /// comienzan con este valor se procesarán para conversión de JSON a XML.
        /// </param>
        /// <param name="addTransformHeader">
        /// Si es <c>true</c>, se añadirá la cabecera <c>Transformed-By: ACL-Proxy</c> a las respuestas convertidas.
        /// </param>
        public JsonToXmlMiddleware(RequestDelegate next, string pathPrefix = "/api/orders", bool addTransformHeader = true)
        {
            _next = next;
            _pathPrefix = pathPrefix;
            _addTransformHeader = addTransformHeader;
        }

        /// <summary>
        /// Ejecuta el middleware para cada petición HTTP.
        /// </summary>
        /// <param name="context">El contexto HTTP actual.</param>
        /// <returns>Una tarea que representa la operación asincrónica.</returns>
        /// <remarks>
        /// Si la ruta de solicitud no coincide con <see cref="_pathPrefix"/>, el middleware delega
        /// inmediatamente al siguiente componente. En caso contrario, captura el cuerpo de la respuesta,
        /// intenta convertir JSON a XML y reemplaza el contenido antes de devolverlo al cliente.
        /// Si ocurre cualquier excepción durante la conversión, se devuelve el contenido original.
        /// </remarks>
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
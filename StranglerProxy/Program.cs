using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using StranglerProxy.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Agregamos YARP leyendo la configuración del appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

//Ejemplo de Anti-Corruption Layer (ACL).
//app.UseJsonToXmlMiddleware("/api/orders", addTransformHeader: true);
// El Proxy intercepta todas las llamadas
app.MapReverseProxy();

app.Run();
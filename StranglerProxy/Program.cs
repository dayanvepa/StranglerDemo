var builder = WebApplication.CreateBuilder(args);

// Agregamos YARP leyendo la configuración del appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// El Proxy intercepta todas las llamadas
app.MapReverseProxy();

app.Run();
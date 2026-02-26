using LegacyApp.Models;
using LegacyApp.Service;
using System.Xml.Serialization;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers()
    .AddXmlSerializerFormatters();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoint que sigue en el monolito
app.MapGet("/api/products", (IProductService productService) => new { 
    
    Source = "Legacy Monolith", 
    Data = productService.GetProducts(),
});

// Endpoint que VAMOS a migrar
app.MapGet("/api/orders", (IOrderService orderService) => new { 
    Source = "Legacy Monolith", 
    Data = orderService.GetOrders(),
});

app.MapGet("/api/ordersOld", (IOrderService orderService) =>
{
    var xml = orderService.GetOrdersXml();
    return Results.Content(xml, "application/xml");
});


app.Run();



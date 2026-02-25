using LegacyApp.Models;
using System.Xml.Serialization;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoint que sigue en el monolito
app.MapGet("/api/products", () => new { 
    Source = "Legacy Monolith", 
    Data = GetProducts(),
});

// Endpoint que VAMOS a migrar
app.MapGet("/api/orders", () => new { 
    Source = "Legacy Monolith", 
    Data = GetOrders(),
});



app.Run();



 List<OrderDto> GetOrders() {
    return new List<OrderDto>
    {
        new OrderDto(101, "Juan Pérez",    DateTime.Now.AddDays(-4), 150.50m, "Completado", 3),
        new OrderDto(102, "María García",   DateTime.Now.AddDays(-3),  85.00m, "Pendiente",   1),
        new OrderDto(103, "Carlos Ruiz",DateTime.Now, 2400.00m, "Completado",   2)
    };
}

 List<ProductDto> GetProducts() {
    return new List<ProductDto>
    {
        new ProductDto(1, "Laptop Pro 15",    "Electrónica", 1200.00m, 15),
        new ProductDto(2, "Mouse Ergonómico",  "Accesorios",  45.99m, 50),
        new ProductDto(3, "Teclado Mecánico RGB", "Accesorios", 129.90m, 20)
    };
}

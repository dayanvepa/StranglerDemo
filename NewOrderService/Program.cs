using NewOrderService.DTOs;

var builder = WebApplication.CreateBuilder(args);


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/orders", () => new { 
    Source = "New Microservice (.NET 10)", 
    Data = GetOrders(),
    Status = "Success"
});

app.Run();


 List<OrderDto> GetOrders() {
    return new List<OrderDto>
    {
        new OrderDto(101, "Juan Pérez",    DateTime.Now.AddDays(-4), 150.50m, "Completado", 3),
        new OrderDto(102, "María García",   DateTime.Now.AddDays(-3),  85.00m, "Pendiente",   1),
        new OrderDto(103, "Carlos Ruiz",, DateTime.Now, 2400.00m, "Completado",   2)
    };
}

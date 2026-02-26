using NewOrderService.Service;

var builder = WebApplication.CreateBuilder(args);


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IOrderService, OrderService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/orders", (IOrderService orderService) => new { 
    Source = "New Microservice (.NET 10)", 
    Data = orderService.GetOrders(),
    Status = "Success"
});

app.Run();


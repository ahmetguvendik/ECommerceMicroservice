using OrderService.Application;
using OrderService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationService(builder.Configuration);
builder.Services.AddPersistenceService(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
using ApiGateway.Middleware;
using Common.Logging;
using EventBus.Messages.Common;
using MassTransit;
using Payment.API.Consumer;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// add serilog package later
builder.Host.UseSerilog(Logging.ConfigureLogging);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<OrderCreatedConsumer>();
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
        cfg.ReceiveEndpoint(EventBusConstant.OrderCreatedQueue, e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(ctx);
        });
    });
});
builder.Services.AddHttpContextAccessor();
var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
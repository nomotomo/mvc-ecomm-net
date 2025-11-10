using Asp.Versioning;
using EventBus.Messages.Common;
using MassTransit;
using Ordering.API.Dispatcher;
using Ordering.API.EventBusConsume;
using Ordering.API.Extensions;
using Ordering.Application.Extensions;
using Ordering.Infrastructure.Data;
using Ordering.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Add Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

builder.Services.AddControllers();
// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

//Application Services
builder.Services.AddApplicationServices();

// Infra Services
builder.Services.AddInfrastructure(builder.Configuration);
//consumer class
builder.Services.AddScoped<BasketOrderingConsumer>();
// Register outbox message dispatcher as a hosted service
builder.Services.AddHostedService<OutBoxMessageDispatcher>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Ordering.API", Version = "v1" }); });
builder.Services.AddHttpContextAccessor();
// MassTransit-RabbitMQ Configuration
builder.Services.AddMassTransit(config => 
{
    config.AddConsumer<BasketOrderingConsumer>();
    config.AddConsumer<PaymentCompletedConsumer>();
    config.AddConsumer<PaymentFailedConsumer>();
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
        cfg.ReceiveEndpoint(EventBusConstant.BasketCheckoutQueue,
            c => { c.ConfigureConsumer<BasketOrderingConsumer>(ctx); });
        cfg.ReceiveEndpoint(EventBusConstant.PaymentCompletedQueue,
            c => { c.ConfigureConsumer<PaymentCompletedConsumer>(ctx); });
        cfg.ReceiveEndpoint(EventBusConstant.PaymentFailedQueue,
            c => { c.ConfigureConsumer<PaymentFailedConsumer>(ctx); });
    });
});

var app = builder.Build();

//Apply db migration
app.MigrateDatabase<OrderContext>((context, services) =>
{
    var logger = services.GetService<ILogger<OrderContextSeed>>();
    OrderContextSeed.SeedAsync(context, logger).Wait();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting(); 
app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();
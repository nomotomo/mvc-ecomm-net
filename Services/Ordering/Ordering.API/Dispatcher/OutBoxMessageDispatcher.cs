using EventBus.Messages.Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Ordering.Infrastructure.Data;

namespace Ordering.API.Dispatcher;

public class OutBoxMessageDispatcher(IServiceProvider serviceProvider, ILogger<OutBoxMessageDispatcher> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("OutBox Message Dispatcher is running at: {time}", DateTimeOffset.Now);
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderContext>();
            var publicEndpoints = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var pendingMessages = await dbContext.OutBoxMessages
                .Where(m => m.ProcessedOn == null)
                .OrderBy(m => m.OccuredOn)
                .Take(20)
                .ToListAsync(stoppingToken);
            logger.LogInformation("Fetched {Count} pending OutBox messages", pendingMessages.Count);
            
            foreach (var message in pendingMessages)
            {
                try 
                {
                    // Here you would typically publish the message to a message broker
                    logger.LogInformation("Processing OutBox Message Id: {MessageId}, Type: {MessageType}", message.Id, message.Type);
                    var orderCreateEvent = JsonConvert.DeserializeObject<OrderCreatedEvent>(message.Content);
                    if (orderCreateEvent != null)
                    {
                        await publicEndpoints.Publish(orderCreateEvent, cancellationToken: stoppingToken);
                        message.ProcessedOn = DateTime.UtcNow;
                        logger.LogInformation("Successfully processed OutBox Message Id: {MessageId}", message.Id);
                    }
                    else
                    {
                        logger.LogWarning("Failed to deserialize OutBox Message Id: {MessageId}", message.Id);
                    }   
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing OutBox Message Id: {MessageId}", message.Id);
                    // Optionally implement retry logic or move the message to a dead-letter queue
                }
            }
            
            await dbContext.SaveChangesAsync(stoppingToken);
            await Task.Delay(5000, stoppingToken);
        }
    }
}
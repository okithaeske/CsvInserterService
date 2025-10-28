using Confluent.Kafka;
using CsvInserterService.Application.Interfaces;
using CsvInserterService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CsvInserterService.Infrastructure.Services;

public class KafkaStudentConsumer : BackgroundService
{
    private readonly ILogger<KafkaStudentConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    // Reintroduce previous field to avoid runtime Edit-and-Continue rename error
    private readonly IStudentInserter _studentInserter = default!;

    public KafkaStudentConsumer(
        ILogger<KafkaStudentConsumer> logger,
        IServiceProvider serviceProvider,
        IConfiguration config)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"],
            GroupId = "student-inserter-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(_config["Kafka:Topic"]);

        _logger.LogInformation("Kafka Consumer started and subscribed.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);

                var student = JsonSerializer.Deserialize<Student>(result.Message.Value);

                if (student != null)
                {
                    // Create a scope to resolve scoped services like AppDbContext/IStudentInserter
                    using var scope = _serviceProvider.CreateScope();
                    var studentInserter = scope.ServiceProvider.GetRequiredService<IStudentInserter>();

                    await studentInserter.InsertAsync(student);
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError($"Kafka error: {ex.Error.Reason}");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while consuming Kafka messages.");
            }
        }

        consumer.Close();
    }
}

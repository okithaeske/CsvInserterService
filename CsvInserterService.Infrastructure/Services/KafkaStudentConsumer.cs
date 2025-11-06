using Confluent.Kafka;
using CsvInserterService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;

namespace CsvInserterService.Infrastructure.Services
{
    public class KafkaStudentConsumer : BackgroundService
    {
        private readonly ILogger<KafkaStudentConsumer> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        public static event Func<string, Task>? OnImportProgress;

        public KafkaStudentConsumer(
            ILogger<KafkaStudentConsumer> logger,
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092";
            var topic = _configuration.GetValue<string>("Kafka:Topic") ?? "student.import";

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "csv-inserter-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(topic);

            _logger.LogInformation("Kafka consumer started and subscribed to {Topic}.", topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    if (string.IsNullOrWhiteSpace(result?.Message?.Value))
                    {
                        continue;
                    }

                    var messageJson = result.Message.Value.Trim();
                    string? filePath = null;

                    try
                    {
                        var json = JsonSerializer.Deserialize<JsonElement>(messageJson);
                        filePath = json.TryGetProperty("path", out var property) ? property.GetString() : null;
                    }
                    catch
                    {
                        filePath = result.Message.Value.Trim('"');
                    }

                    if (string.IsNullOrEmpty(filePath))
                    {
                        _logger.LogWarning("Invalid file path message received from Kafka.");
                        continue;
                    }

                    await (OnImportProgress?.Invoke($"Received file for import: {filePath}") ?? Task.CompletedTask);

                    using var scope = _scopeFactory.CreateScope();
                    var csvImportService = scope.ServiceProvider.GetRequiredService<ICsvImportService>();

                    if (File.Exists(filePath))
                    {
                        await (OnImportProgress?.Invoke($"Started importing {Path.GetFileName(filePath)}...") ?? Task.CompletedTask);
                        await csvImportService.ImportCsvAsync(filePath);
                        await (OnImportProgress?.Invoke($"Import completed for {Path.GetFileName(filePath)}") ?? Task.CompletedTask);
                    }
                    else
                    {
                        await (OnImportProgress?.Invoke($"File not found: {filePath}") ?? Task.CompletedTask);
                        _logger.LogWarning("File not found: {FilePath}", filePath);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError("Kafka consume error: {Reason}", ex.Error.Reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception while consuming Kafka messages.");
                }
            }

            consumer.Close();
        }
    }
}

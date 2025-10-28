using CsvInserterService.Application.Interfaces;
using CsvInserterService.Infrastructure.Data;
using CsvInserterService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;



var builder = WebApplication.CreateBuilder(args);

// Add Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

// Add Kafka consumer and student inserter
builder.Services.AddScoped<IStudentInserter, StudentInserter>();
builder.Services.AddHostedService<KafkaStudentConsumer>();

// (Optional) Add minimal endpoint
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));


var app = builder.Build();

app.MapGet("/", () => "CsvInserterService is running!");
app.Run();

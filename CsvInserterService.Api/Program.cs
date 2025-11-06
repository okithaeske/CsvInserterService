using CsvInserterService.Application.Interfaces;
using CsvInserterService.Infrastructure.Data;
using CsvInserterService.Infrastructure.Services;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

// Register services
builder.Services.AddScoped<IStudentInserter, StudentInserter>();                                                                             
builder.Services.AddScoped<ICsvImportService, CsvImportService>();   
builder.Services.AddScoped<CsvImportService>();                                                                        
builder.Services.AddHostedService<KafkaStudentConsumer>();  

builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Add CORS policy for Angular localhost
var corsPolicy = "_allowAngular";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy, policy =>
    {
        policy
            .AllowAnyOrigin() // for dev; replace with specific origins later
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ✅ Add WebSocket support
builder.Services.AddWebSockets(options => { });

var app = builder.Build();

// ✅ Ensure proper order
app.UseCors(corsPolicy);
app.UseWebSockets();

// 🔍 Log all requests (for debugging)
app.Use(async (context, next) =>
{
    Console.WriteLine($"➡️ Incoming: {context.Request.Method} {context.Request.Path}");
    await next();
});

// Track connected WebSocket clients
var importSockets = new List<WebSocket>();

// ✅ WebSocket endpoint
app.Map("/ws/import", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.WebSockets.AcceptWebSocketAsync();
        importSockets.Add(ws);
        Console.WriteLine("✅ WebSocket client connected to /ws/import");

        var buffer = new byte[1024 * 4];
        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                importSockets.Remove(ws);
                Console.WriteLine("⚠️ WebSocket client disconnected from /ws/import");
            }
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

// Helper: broadcast message to all clients
async Task BroadcastImportStatusAsync(string message)
{
    var buffer = Encoding.UTF8.GetBytes(message);
    var segment = new ArraySegment<byte>(buffer);

    foreach (var ws in importSockets.ToList())
    {
        if (ws.State == WebSocketState.Open)
            await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}

// Test API
app.MapGet("/api/students/csvdata", async (ApplicationDbContext db) =>
{
    var students = await db.Students.ToListAsync();
    return Results.Ok(students);
});

// Kafka hook
KafkaStudentConsumer.OnImportProgress += async (status) =>
{
    var payload = JsonSerializer.Serialize(new { status });
    await BroadcastImportStatusAsync(payload);
};

// ✅ Ensure app listens on all interfaces (for Docker/Angular host)
app.Urls.Add("http://0.0.0.0:5144");

app.Run();




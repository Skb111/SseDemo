using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Allows the app to serve our index.html file from the wwwroot folder
app.UseDefaultFiles();
app.UseStaticFiles();

// The SSE Endpoint
app.MapGet("/stream-prices", async (HttpContext context, CancellationToken ct) =>
{
    // 1. Set the required SSE header
    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");

    var random = new Random();
    var stocks = new[] { "MSFT", "AAPL", "GOOG", "AMZN" };

    // 2. Keep the connection open until the user closes the browser tab
    while (!ct.IsCancellationRequested)
    {
        // Create a sample C# object
        var update = new
        {
            Symbol = stocks[random.Next(stocks.Length)],
            Price = Math.Round(100 + (random.NextDouble() * 50), 2),
            Timestamp = DateTime.Now.ToString("HH:mm:ss")
        };

        // 3. Serialize the object to JSON
        string jsonPayload = JsonSerializer.Serialize(update);

        // 4. Format strictly as "data: <json>\n\n" and send it
        await context.Response.WriteAsync($"data: {jsonPayload}\n\n", ct);
        await context.Response.Body.FlushAsync(ct);

        // Wait 2 seconds before sending the next update
        await Task.Delay(2000, ct);
    }
});

app.Run();

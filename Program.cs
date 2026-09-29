var builder = WebApplication.CreateBuilder(args);

// THIS is the line Mercya's ASP.NET Core buildpack requires, along with the
// Amazon.Lambda.AspNetCoreServer.Hosting package reference in the .csproj.
// It adapts Lambda's incoming event into the same HttpContext the rest of
// this app already handles, so nothing below has to know it runs on Lambda.
//
// HttpApi, not RestApi: Mercya fronts the function with a Lambda Function
// URL, whose payload shape matches API Gateway's HTTP API v2 - the same
// choice Mercya's own API makes for itself.
//
// Harmless outside Lambda: the package no-ops when AWS_LAMBDA_FUNCTION_NAME
// is absent, so `dotnet run` locally still uses Kestrel.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// A real HTML root, not just JSON: Mercya takes a best-effort screenshot of
// the deployed site, and an API-only app gives it nothing to capture.
app.MapGet("/", () => Results.Content("""
    <!doctype html>
    <title>Deployed on Mercya</title>
    <style>
      body { font-family: system-ui, sans-serif; display: grid; place-items: center;
             min-height: 100vh; margin: 0; background: #0b0b0b; color: #e8e8e8; }
      main { text-align: center; }
      h1 { font-size: 2rem; margin: 0 0 .5rem; }
      p { color: #9a9a9a; margin: .25rem 0; }
      code { background: #1a1a1a; padding: .15rem .4rem; border-radius: 4px; }
    </style>
    <main>
      <h1>ASP.NET Core on Mercya</h1>
      <p>Running on AWS Lambda behind CloudFront.</p>
      <p>Try <code>/weatherforecast</code> or <code>/health</code>.</p>
    </main>
    """, "text/html"));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    // Proves the platform's own environment wiring reached the app -
    // AspNetCoreEnvironmentStack injects this from the Domain parameter.
    domain = Environment.GetEnvironmentVariable("MERCYA_DOMAIN") ?? "(not set)",
    runtime = Environment.Version.ToString()
}));

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]))
        .ToArray())
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

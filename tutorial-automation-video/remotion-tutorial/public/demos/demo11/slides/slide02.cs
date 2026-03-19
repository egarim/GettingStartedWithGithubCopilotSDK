#:package GitHub.Copilot.SDK@0.1.23
#:package Microsoft.Extensions.Logging.Console@*

// Paso 1: Iniciar cliente BYOK y verificar auth
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
var client = new CopilotClient(new CopilotClientOptions
{
    GithubToken = string.IsNullOrWhiteSpace(token) ? null : token,
    UseLoggedInUser = string.IsNullOrWhiteSpace(token),  // fallback a VS Code login
    Logger = loggerFactory.CreateLogger<CopilotClient>()
});
await client.StartAsync();

var auth = await client.GetAuthStatusAsync();
Console.WriteLine($"  Auth: {auth.IsAuthenticated} ({auth.AuthType})");
Console.WriteLine($"  Fuente: {(string.IsNullOrWhiteSpace(token) ? "VS Code login" : "GitHub PAT")}");

await client.StopAsync();
await client.DisposeAsync();
using ClientDemo;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;

// ============================================================================
// 01 – CLIENT DEMO: Client Lifecycle & Connection
// ============================================================================

using var loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Warning));

var ui = new ConsoleRenderer();
var runner = new ClientDemoRunner(ui, loggerFactory.CreateLogger<CopilotClient>());
await runner.RunAsync();

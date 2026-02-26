namespace ClientDemo;

/// <summary>
/// Handles all console I/O: headers, steps, properties, prompts, and formatting.
/// Language-aware (English / Spanish).
/// </summary>
public sealed class ConsoleRenderer
{
    public bool IsSpanish { get; private set; }

    // ── Language selection ───────────────────────────────────────────────
    public void SelectLanguage()
    {
        ShowBanner("  Select language / Seleccione idioma:");
        Console.WriteLine("  1. English");
        Console.WriteLine("  2. Español");
        ShowDivider();
        Console.Write("  Choice (1 or 2): ");
        IsSpanish = Console.ReadLine()?.Trim() == "2";
        Console.WriteLine();
    }

    // ── Structural helpers ──────────────────────────────────────────────
    public void ShowDemoTitle(string enTitle, string esTitle)
    {
        ShowDivider();
        Console.WriteLine(IsSpanish ? $"  {esTitle}" : $"  {enTitle}");
        ShowDivider();
        Console.WriteLine();
    }

    public void ShowStep(int step, string enText, string esText)
    {
        Console.WriteLine(IsSpanish
            ? $"=== {step}. {esText} ==="
            : $"=== {step}. {enText} ===");
    }

    public void ShowProperty(string enLabel, string esLabel, string value)
    {
        var label = IsSpanish ? esLabel : enLabel;
        Console.WriteLine($"  {label,-20} {value}");
    }

    public void ShowInfo(string enText, string esText)
        => Console.WriteLine(IsSpanish ? $"  {esText}" : $"  {enText}");

    public void ShowLine(string text)
        => Console.WriteLine(text);

    public void BlankLine()
        => Console.WriteLine();

    // ── Models table ────────────────────────────────────────────────────
    public void ShowModelsHeader()
    {
        Console.WriteLine($"  {"ID",-35} {"Name",-25} {"Capabilities"}");
        Console.WriteLine($"  {"--",-35} {"----",-25} {"------------"}");
    }

    public void ShowModelRow(string id, string name, string capabilities)
        => Console.WriteLine($"  {id,-35} {name,-25} {capabilities}");

    // ── Prompts ─────────────────────────────────────────────────────────
    public string? ReadLine()
        => Console.ReadLine();

    public string? Prompt(string text)
    {
        Console.Write(text);
        return Console.ReadLine();
    }

    public void WaitForEnterOrExit()
    {
        ShowDivider();
        ShowInfo(
            "Press Enter for interactive mode, or Ctrl+C to exit.",
            "Presiona Enter para modo interactivo, o Ctrl+C para salir.");
        ShowDivider();
        Console.ReadLine();
    }

    public void ShowInteractiveHelp()
    {
        ShowInfo(
            "Interactive commands: ping, status, auth, models, quit",
            "Comandos: ping, status, auth, models, quit");
        BlankLine();
    }

    public void ShowInteractivePrompt()
        => Console.Write("  > ");

    public void ShowInteractiveResult(string text)
        => Console.WriteLine($"    → {text}");

    public void ShowUnknownCommand()
        => ShowInfo(
            "Unknown command. Try: ping, status, auth, models, quit",
            "Comando desconocido. Prueba: ping, status, auth, models, quit");

    public void ShowError(string message)
        => Console.WriteLine($"    Error: {message}");

    public void ShowDone()
        => Console.WriteLine(IsSpanish ? "\n  ¡Listo!" : "\n  Done!");

    // ── Private ─────────────────────────────────────────────────────────
    private void ShowDivider()
        => Console.WriteLine("================================================================");

    private void ShowBanner(string text)
    {
        ShowDivider();
        Console.WriteLine(text);
    }
}

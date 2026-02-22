using BlazorChatDemo.Components;
using BlazorChatDemo.Data;
using BlazorChatDemo.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── EF Core + SQLite ──────────────────────────────────────────────────
builder.Services.AddDbContextFactory<NorthwindDbContext>(options =>
    options.UseSqlite("Data Source=northwind.db"));

// ── GitHub Copilot SDK services ────────────────────────────────────────
builder.Services.AddCopilotSdk(builder.Configuration);

// ── Blazor ─────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ── Seed the database ──────────────────────────────────────────────────
await NorthwindSeeder.SeedAsync(app.Services);

// ── Middleware ──────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

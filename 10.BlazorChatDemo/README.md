# 10 – Blazor Chat Demo: Full-Stack AI Chat with Northwind Data

## Concept

This demo brings together **everything from the course** into a complete, interactive Blazor Server application. It connects a rich browser-based chat UI to the **GitHub Copilot SDK**, backed by a **Northwind-style database** seeded with realistic business data.

The AI assistant can query orders, analyze invoices, check stock levels, review employee performance, and even create new orders — all through natural conversation.

| # | Topic | Description |
|---|-------|-------------|
| 1 | **Blazor Server Chat UI** | Custom chat interface with message list, suggestions, and markdown rendering |
| 2 | **EF Core + SQLite** | Standalone data layer with 13 entities and auto-seeding — no external DB required |
| 3 | **IChatClient Adapter** | `CopilotChatClient` wraps `CopilotChatService` as a standard `IChatClient` |
| 4 | **AI Tools (6 functions)** | `query_orders`, `invoice_aging`, `low_stock_products`, `employee_order_stats`, `employee_territories`, `create_order` |
| 5 | **System Prompt** | Detailed Northwind schema description so the model understands the data |
| 6 | **Markdown → HTML** | Responses rendered with Markdig + HtmlSanitizer for tables, lists, and formatting |
| 7 | **Prompt Suggestions** | Pre-built suggestion cards guide users toward useful queries |
| 8 | **Dependency Injection** | `IDbContextFactory<NorthwindDbContext>` for thread-safe DB access in tools |

## Architecture

```
┌─────────────────────────────────────────────────────┐
│  Browser — Blazor Interactive Server                │
│  ┌──────────┐ ┌─────────────┐ ┌──────────────────┐  │
│  │ChatInput │→│  Chat.razor  │→│ChatMessageList   │  │
│  └──────────┘ └──────┬──────┘ └──────────────────┘  │
│                      │                               │
│              IChatClient.GetResponseAsync()           │
│                      │                               │
├──────────────────────┼───────────────────────────────┤
│  Server              │                               │
│  ┌───────────────────▼────────────────────────────┐  │
│  │         CopilotChatClient (IChatClient)        │  │
│  │                    │                           │  │
│  │         CopilotChatService (Singleton)         │  │
│  │           ┌────────┴────────┐                  │  │
│  │     CopilotClient     NorthwindToolsProvider   │  │
│  │     (GitHub SDK)      (6 AI tools via EF Core) │  │
│  └───────────────────────────────────────────────┘  │
│                      │                               │
│  ┌───────────────────▼────────────────────────────┐  │
│  │   NorthwindDbContext (SQLite — northwind.db)   │  │
│  │   13 entities · Northwind seed data            │  │
│  └────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

## Key Code

### Service Registration (`Program.cs`)

```csharp
// EF Core + SQLite
builder.Services.AddDbContextFactory<NorthwindDbContext>(options =>
    options.UseSqlite("Data Source=northwind.db"));

// GitHub Copilot SDK (registers CopilotChatService, tools, IChatClient)
builder.Services.AddCopilotSdk(builder.Configuration);

// Seed the database on startup
await NorthwindSeeder.SeedAsync(app.Services);
```

### AI Tool Example (`NorthwindToolsProvider.cs`)

```csharp
[Description("Search orders by customer name and/or status.")]
private string QueryOrders(
    [Description("Customer company name (partial match).")] string customerName = "",
    [Description("Order status filter.")] string status = "")
{
    using var db = _dbFactory.CreateDbContext();
    var query = db.Orders
        .Include(o => o.Customer)
        .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
        .AsQueryable();
    // ... filter, project, return formatted text
}
```

### Chat Page (`Chat.razor`)

```razor
@inject IChatClient ChatClient

<ChatHeader Title="@CopilotChatDefaults.HeaderText" />
<ChatSuggestions Suggestions="CopilotChatDefaults.PromptSuggestions"
                 OnSuggestionClicked="HandleSuggestionClicked" />
<ChatMessageList Messages="_messages" IsThinking="_isThinking" />
<ChatInput OnMessageSent="HandleMessageSent" IsDisabled="_isThinking" />
```

## Data Model

13 entities modeled after **Northwind**: Category, Customer, Employee, EmployeeTerritory, Invoice, Order, OrderItem, Product, Region, Shipper, Supplier, Territory — with full relationships and referential integrity.

Seed data: 4 regions, 12 territories, 5 employees, 8 categories, 10 suppliers, 30 products, 20 customers, 3 shippers, 50 orders, 20 invoices.

## Running

```bash
dotnet run --project Course/demos/10.BlazorChatDemo
```

Then open **https://localhost:5101** in your browser. The SQLite database is created and seeded automatically on first launch.

> **Prerequisite:** You must be authenticated with GitHub Copilot (VS Code or `gh auth login`).

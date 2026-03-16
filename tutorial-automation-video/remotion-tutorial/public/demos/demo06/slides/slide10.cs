// ...
var requestsWithChoices = userInputRequests.Where(r => r.Choices is { Count: > 0 }).ToList();
PrintProp("Con opciones:", requestsWithChoices.Count);
await session.DisposeAsync();
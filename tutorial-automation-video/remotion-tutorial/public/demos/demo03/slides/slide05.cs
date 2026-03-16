// Tipos complejos de entrada/salida
ToolInvocation? receivedInvocation = null;

City[] PerformDbQuery(DbQueryOptions query, AIFunctionArguments rawArgs)
{
    Console.WriteLine($"    [Tool called] Table={query.Table}, IDs=[{string.Join(",", query.Ids)}], Sort={query.SortAscending}");
    receivedInvocation = (ToolInvocation)rawArgs.Context![typeof(ToolInvocation)]!;
    return [new(19, "Passos", 135460), new(12, "San Lorenzo", 204356)];
}
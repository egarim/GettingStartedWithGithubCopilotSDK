// ...
var session = await client.CreateSessionAsync(new SessionConfig
{
    Tools = [AIFunctionFactory.Create(PerformDbQuery, "db_query",
        serializerOptions: DemoJsonContext.Default.Options)]
});
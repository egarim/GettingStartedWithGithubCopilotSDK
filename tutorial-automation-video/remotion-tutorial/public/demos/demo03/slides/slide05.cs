// Paso 4: Tipos complejos de entrada/salida (records, arrays)
City[] PerformDbQuery(DbQueryOptions query, AIFunctionArguments rawArgs)
    Console.WriteLine($"  [Tool] Table={query.Table}, IDs=[{string.Join(",", query.Ids)}]");
    return [new(19, "Passos", 135460), new(12, "San Lorenzo", 204356)];
}
    Tools = [AIFunctionFactory.Create(PerformDbQuery, "db_query",
        serializerOptions: DemoJsonContext.Default.Options)]
var answer = await session.SendAndWaitAsync(new MessageOptions
    Prompt = "Query the 'cities' table with IDs 12 and 19, sorting ascending."
Console.WriteLine($"Respuesta: {answer?.Data.Content}"); // -> ciudades con poblacion
record DbQueryOptions(string Table, int[] Ids, bool SortAscending);
record City(int CountryId, string CityName, int Population);
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(DbQueryOptions))]
[JsonSerializable(typeof(City[]))]
[JsonSerializable(typeof(JsonElement))]
partial class DemoJsonContext : JsonSerializerContext;
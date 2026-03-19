// Paso 4: Configuracion de agente personalizado
    CustomAgents = new List<CustomAgentConfig>
        new CustomAgentConfig
            Name = "business-analyst",
            DisplayName = "Business Analyst Agent",
            Description = "An agent specialized in business analysis",
            Prompt = "You are a business analyst. Focus on data-driven insights and KPIs.",
            Infer = true  // el modelo decide cuando usar este agente
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is 5+5?" });
Console.WriteLine($"  Respuesta: {answer?.Data.Content}");
// Paso 5: Agente con herramientas especificas
            Name = "devops-agent",
            DisplayName = "DevOps Agent",
            Description = "An agent for DevOps tasks",
            Prompt = "You are a DevOps agent. You can use bash and edit tools.",
            Tools = ["bash", "edit"],  // conjunto restringido de herramientas
            Infer = true
Console.WriteLine($"  Agente devops con Tools: [\"bash\", \"edit\"]");
// Paso 7: Multiples agentes personalizados
            Name = "frontend-agent",
            DisplayName = "Frontend Agent",
            Description = "Specializes in React, CSS, and UI",
            Prompt = "You are a frontend development expert."
        },
            Name = "backend-agent",
            DisplayName = "Backend Agent",
            Description = "Specializes in C#, .NET, and APIs",
            Prompt = "You are a backend development expert.",
            Infer = false  // solo se invoca explicitamente
Console.WriteLine("  2 agentes configurados: frontend-agent, backend-agent");
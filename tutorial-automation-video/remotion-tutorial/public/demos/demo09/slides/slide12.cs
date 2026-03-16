// Agente con herramientas
var customAgents = new List<CustomAgentConfig>
{
    new CustomAgentConfig
    {
        Name = "devops-agent",
        DisplayName = "DevOps Agent",
        Description = "An agent for DevOps tasks with specific tool access",
        Prompt = "You are a DevOps agent. You can use bash and edit tools.",
        Tools = ["bash", "edit"],
        Infer = true
    }
};
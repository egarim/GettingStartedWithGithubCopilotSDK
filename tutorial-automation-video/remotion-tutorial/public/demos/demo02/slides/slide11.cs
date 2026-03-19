// Paso 10: Mensaje de sistema - Modo Replace
        Mode = SystemMessageMode.Replace,
        Content = "You are an assistant called Testy McTestface. Reply succinctly."
var answer = await session.SendAndWaitAsync(new MessageOptions { Prompt = "What is your full name?" });
// -> Menciona "Testy McTestface" en lugar de "GitHub Copilot"
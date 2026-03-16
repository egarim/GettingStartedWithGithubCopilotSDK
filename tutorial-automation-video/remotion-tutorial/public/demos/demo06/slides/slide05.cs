// ...
        Console.WriteLine("    [AskUser] Sin opciones — respuesta libre");
        return Task.FromResult(new UserInputResponse
        {
            Answer = "I'll go with the default option",
            WasFreeform = true
        });
    }
});
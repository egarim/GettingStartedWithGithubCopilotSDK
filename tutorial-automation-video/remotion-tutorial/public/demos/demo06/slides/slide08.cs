// ...
        var answer = request.Choices?.FirstOrDefault() ?? "default";
        return Task.FromResult(new UserInputResponse { Answer = answer, WasFreeform = false });
    }
});
// ...
session.On(evt =>
{
    switch (evt)
    {
        case AssistantMessageDeltaEvent delta:
            Console.Write(delta.Data.DeltaContent);
            sb.Append(delta.Data.DeltaContent);
            break;
        case SessionIdleEvent:
            idleTcs.TrySetResult(true);
            break;
    }
});
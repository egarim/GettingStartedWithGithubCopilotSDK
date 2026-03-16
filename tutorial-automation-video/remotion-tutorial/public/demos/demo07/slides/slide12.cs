// ...
session.On(evt =>
{
    if (evt is SessionCompactionStartEvent or SessionCompactionCompleteEvent)
    {
        compactionEvents.Add(evt);
    }
});
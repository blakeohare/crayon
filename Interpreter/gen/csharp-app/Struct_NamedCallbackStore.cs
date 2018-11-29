public class NamedCallbackStore
{
    public List<Func<object[], object>> callbacksById;
    public Dictionary<string, Dictionary<string, int>> callbackIdLookup;

    public NamedCallbackStore(List<Func<object[], object>> callbacksById, Dictionary<string, Dictionary<string, int>> callbackIdLookup)
    {
        this.callbacksById = callbacksById;
        this.callbackIdLookup = callbackIdLookup;
    }
}

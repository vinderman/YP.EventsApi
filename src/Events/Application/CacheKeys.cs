namespace Application;

public static class CacheKeys
{
    public static string EventById(Guid id) => $"event:{id}";
    public static string TopSoldEvents = $"events:top10";
}   
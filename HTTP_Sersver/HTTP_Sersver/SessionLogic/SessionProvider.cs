using Microsoft.Extensions.Caching.Memory;

namespace NetConsoleApp.SessionLogic;

public class SessionProvider
{
    private static readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public static Guid CreateSession(int accountId, string nickname, DateTime created)
    {
        var session = new Session(Guid.NewGuid(), accountId, nickname, created);
        var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(2));
        _cache.Set(session.Id, session, cacheOptions);
        return session.Id;
    }

    public static bool CheckSession(Guid id) => _cache.TryGetValue(id, out _);

    public static Session? GetSessionInfo(Guid id) => CheckSession(id) ? _cache.Get(id) as Session : null;
}
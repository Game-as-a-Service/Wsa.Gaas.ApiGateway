using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace LobbyPlatform.ApiGateway;

public class DistributedCacheTicketStore : ITicketStore
{
    private const string KeyPrefix = "auth-";
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;

    public DistributedCacheTicketStore(IMemoryCache cache, ILogger<DistributedCacheTicketStore> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = KeyPrefix + Guid.NewGuid();

        var expires_at = ticket.Properties.GetTokenValue("expires_at");
        var expiresUtc = DateTimeOffset.Parse(expires_at!);

        var options = new MemoryCacheEntryOptions();
        options.SetAbsoluteExpiration(expiresUtc);

        // Cache expiry should equal refresh_token expiry
        _cache.Set(key, ticket, options);

        _logger.LogError("Cache {key} expires at {expiresUtc}", key, expiresUtc);
        _logger.LogError("Ticket {key} expires at {expiresUtc}", key, ticket.Properties.ExpiresUtc);

        return Task.FromResult(key);
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var options = new MemoryCacheEntryOptions();
        var expires_at = ticket.Properties.GetTokenValue("expires_at");
        var expiresUtc = DateTimeOffset.Parse(expires_at!);

        options.SetAbsoluteExpiration(expiresUtc);
        _cache.Set(key, ticket, options);

        _logger.LogError("Cache {key} expires at {expiresUtc}", key, expiresUtc);
        _logger.LogError("Ticket {key} expires at {expiresUtc}", key, ticket.Properties.ExpiresUtc);

        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        return Task.FromResult(_cache.Get<AuthenticationTicket>(key));
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);

        return Task.CompletedTask;
    }

    
}

namespace Application.Interfaces;

public interface ICacheService
{
    public Task<T?> GetByKey<T>(string key) where T : class;
    
    public Task Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    
    public Task Delete(string key);
}
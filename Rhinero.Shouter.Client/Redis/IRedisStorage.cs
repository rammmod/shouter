namespace Rhinero.Shouter.Client.Redis
{
    public interface IRedisStorage
    {
        Task<string> GetAsync(string key);
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
    }
}
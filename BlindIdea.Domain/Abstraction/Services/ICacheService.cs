using System;
using System.Threading.Tasks;

namespace BlindIdea.Domain.Abstraction.Services
{
    public interface ICacheService
    {
        Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> fetchFromDb,
            TimeSpan? duration = null);

        void Remove(string key);

        void RemoveMany(params string[] keys);

        bool Exists(string key);

        void Set<T>(string key, T value, TimeSpan? duration = null);
    }
}

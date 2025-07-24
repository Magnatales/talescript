#nullable enable
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.TaleScript.Libraries.Storage;

public interface IStorage
{
    // For stats (integers, incrementable)
    Task<int> GetStatAsync(string key);
    Task SetStatAsync(string key, int value);
    Task IncrementStatAsync(string key, int amount = 1);
    Task<bool> StatExistsAsync(string key);
    Task DeleteStatAsync(string key);

    // For flags or events (booleans)
    Task<bool> GetFlagAsync(string key);
    Task SetFlagAsync(string key, bool value);
    Task<bool> FlagExistsAsync(string key);
    Task DeleteFlagAsync(string key);
    
    //String
    Task<string?> GetStringAsync(string key);
    Task SetStringAsync(string key, string value);
    Task<bool> StringExistsAsync(string key);
    Task DeleteStringAsync(string key);


    public static IStorage Dummy { get; } = new DummyStorage();
    private class DummyStorage : IStorage
    {
        private readonly ConcurrentDictionary<string, int> _stats = new();
        private readonly ConcurrentDictionary<string, bool> _flags = new();
        private readonly ConcurrentDictionary<string, string> _strings = new();

        // Stats
        public Task<int> GetStatAsync(string key) =>
            _stats.TryGetValue(key, out var value)
                ? Task.FromResult(value)
                : throw new KeyNotFoundException($"Stat '{key}' not found.");

        public Task SetStatAsync(string key, int value)
        {
            _stats[key] = value;
            return Task.CompletedTask;
        }

        public Task IncrementStatAsync(string key, int amount = 1)
        {
            _stats.AddOrUpdate(key, amount, (_, old) => old + amount);
            return Task.CompletedTask;
        }

        public Task<bool> StatExistsAsync(string key) => Task.FromResult(_stats.ContainsKey(key));

        public Task DeleteStatAsync(string key)
        {
            _stats.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        // Flags
        public Task<bool> GetFlagAsync(string key) =>
            _flags.TryGetValue(key, out var value)
                ? Task.FromResult(value)
                : throw new KeyNotFoundException($"Flag '{key}' not found.");

        public Task SetFlagAsync(string key, bool value)
        {
            _flags[key] = value;
            return Task.CompletedTask;
        }

        public Task<bool> FlagExistsAsync(string key) => Task.FromResult(_flags.ContainsKey(key));

        public Task DeleteFlagAsync(string key)
        {
            _flags.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        // Strings
        public Task<string?> GetStringAsync(string key) =>
            _strings.TryGetValue(key, out var value)
                ? Task.FromResult<string?>(value)
                : Task.FromResult<string?>(null);

        public Task SetStringAsync(string key, string value)
        {
            _strings[key] = value;
            return Task.CompletedTask;
        }

        public Task<bool> StringExistsAsync(string key) => Task.FromResult(_strings.ContainsKey(key));

        public Task DeleteStringAsync(string key)
        {
            _strings.TryRemove(key, out _);
            return Task.CompletedTask;
        }
    }
}
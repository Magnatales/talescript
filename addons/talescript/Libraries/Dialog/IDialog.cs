using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Code.TaleScript.Libraries.Dialog;

public interface IDialog
{
    Task SayAsync(string key, CancellationToken ct = default);
    Task<int> ChoicesAsync(List<string> keys, CancellationToken ct = default);
    
    public static IDialog Dummy { get; } = new DummyDialog();
    private class DummyDialog : IDialog
    {
        public Task SayAsync(string key, CancellationToken ct = default)
        {
            LogUtils.PrintWithTrace(key);
            return Task.CompletedTask;
        }

        public Task<int> ChoicesAsync(List<string> keys, CancellationToken ct = default)
        {
            LogUtils.PrintWithTrace("Choices: " + string.Join(", ", keys));
            var randomChoiceIndex = GD.RandRange(1, keys.Count); // Lua uses 1-based indexing :)
            return Task.FromResult(randomChoiceIndex); // Always return random choice for dummy
        }
    }
}
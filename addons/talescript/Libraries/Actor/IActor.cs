using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Code.TaleScript.Runtime.Utils;
using Godot;

namespace Code.TaleScript.Libraries.Actor;

public interface IActor
{
    string Name { get; }
    Task MoveAsync(float x, float y, CancellationToken ct = default);
    void Teleport(float x, float y);
    void PlayAnimation(string animationName);
    
    
    public static IActor Dummy { get; } = new DummyActor();
    private class DummyActor : IActor
    {
        private Vector2 _position;

        public string Name { get; } = "DummyActor";

        public async Task MoveAsync(float x, float y, CancellationToken ct = default)
        {
            _position = new Vector2(x, y);
            LogUtils.PrintWithTrace($"{nameof(DummyActor)} moved to x:{_position.X} and y:{_position.Y}");
            await GDTask.Delay(0.5f, ct);
        }

        public void Teleport(float x, float y)
        {
            _position = new Vector2(x, y);
            LogUtils.PrintWithTrace($"{nameof(DummyActor)} teleported to x:{_position.X} and y:{_position.Y}");
        }

        public void PlayAnimation(string animationName)
        {
            LogUtils.PrintWithTrace($"Actor {((IActor)this).Name} played {animationName}");
        }
    }
}


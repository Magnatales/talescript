using System.Collections.Generic;
using Godot;

namespace Code.TaleScript.Libraries.Actor;

public interface IActorProvider
{
    void Add(IActor actor);
    IActor GetActor(string name);
    List<IActor> GetActors();
    bool HasActor(string name);
    
    
    public static IActorProvider Dummy { get; } = new DummyActorProvider();
    private class DummyActorProvider : IActorProvider
    {
        private readonly Dictionary<string, IActor> _actors = new()
        {
            { "DummyActor", IActor.Dummy }
        };
    
        public void Add(IActor actor)
        {
            GD.PushError("DummyActorProvider does not support adding actors.");
        }

        public IActor GetActor(string name)
        {
            return _actors["DummyActor"];
        }

        public List<IActor> GetActors()
        {
            return [.._actors.Values];
        }

        public bool HasActor(string name)
        {
            return _actors.ContainsKey(name);
        }
    }
}
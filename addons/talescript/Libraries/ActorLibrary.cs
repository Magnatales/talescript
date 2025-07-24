using System;
using System.Threading;
using System.Threading.Tasks;
using Code.TaleScript.Libraries.Actor;

namespace Lua.Libraries;

public sealed class ActorLibrary
{
    public static readonly ActorLibrary Instance = new();

    public readonly LuaFunction[] Functions;
    
    private IActorProvider _actorProvider;

    public ActorLibrary()
    {
        Functions = [
            new("move", Move),
        ];
    }
    
    public void Initialize(IActorProvider actorProvider)
    {
        _actorProvider = actorProvider ?? throw new ArgumentNullException(nameof(actorProvider));
    }
    
    public async ValueTask<int> Move(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken ct)
    {
        var actorName = context.GetArgument<string>(0);
        var dx = context.GetArgument<float>(1);
        var dy = context.GetArgument<float>(2);
        var actor = _actorProvider.GetActor(actorName);
        await actor.MoveAsync(dx, dy, ct);
        return 0;
    }
}
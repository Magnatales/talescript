using System;
using System.Threading;
using System.Threading.Tasks;
using Code.TaleScript.Runtime.Utils;

namespace Lua.Libraries;

public sealed class AsyncLibrary
{
    public static readonly AsyncLibrary Instance = new();

    public readonly LuaFunction[] Functions;

    public AsyncLibrary()
    {
        Functions = [
            new("wait", Wait),
            new("yield", Yield),
            new ("wait_frames", WaitFrames),
        ];
    }

    public async ValueTask<int> Wait(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken ct)
    {
        var seconds = context.GetArgument<double>(0);
        await GDTask.Delay((float)seconds, ct);
        return 0;
    }

    public async ValueTask<int> Yield(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken ct)
    {
        await GDTask.Yield(ct);
        return 0;
    }
    
    public async ValueTask<int> WaitFrames(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken ct)
    {
        var amount = context.GetArgument<int>(0);
        await GDTask.WaitFrames(amount, ct);
        return 0;
    }
}
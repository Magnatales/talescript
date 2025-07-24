using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Code.TaleScript.Libraries.Dialog;
using Code.TaleScript.Libraries.Storage;
using Code.TaleScript.Extensions;

namespace Lua.Libraries;

public sealed class DialogLibrary
{
    public static readonly DialogLibrary Instance = new();
    public readonly LuaFunction[] Functions;
    
    private IStorage _storage;
    private IDialog _dialog;

    public DialogLibrary()
    {
        Functions = [
            new("say", SayAsync),
            new("say_once", SayOnceAsync),
            new("once", OnceAsync),
            new("detour", DetourAsync),
            new("choices", Choices),
        ];
    }

    public void Initialize(IStorage storage, IDialog dialog)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
    }

    public async ValueTask<int> SayAsync(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken ct)
    {
        var key = context.GetArgument<string>(0);
        await _dialog.SayAsync(key, ct);
        return 0;
    }

    public async ValueTask<int> SayOnceAsync(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken ct)
    {
        var key = context.GetArgument<string>(0);
        if (await _storage.FlagExistsAsync(key))
        {
            buffer.Span[0] = true;
            return 1;
        }

        await _dialog.SayAsync(key, ct);
        await _storage.SetFlagAsync(key, true);
        buffer.Span[0] = false;
        return 1;
    }

    public async ValueTask<int> Choices(LuaFunctionExecutionContext context, Memory<LuaValue> buffer,
        CancellationToken ct)
    {
        var choices = new List<string>();
        for (var i = 0; i < context.Arguments.Length; i++)
        {
            if (context.Arguments[i].Type == LuaValueType.String)
            {
                choices.Add(context.Arguments[i].ToString());
            }
            else
            {
                throw new ArgumentException($"Argument {i} is not a string.");
            }
        }
        var selected = await _dialog.ChoicesAsync(choices, ct);  
        buffer.Span[0] = selected;
        return 1;
    }


    public async ValueTask<int> OnceAsync(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken ct)
    {
        var key = context.GetArgument<string>(0);
        if (await _storage.FlagExistsAsync(key))
        {
            buffer.Span[0] = true;
            return 1;
        }

        await _storage.SetFlagAsync(key, true);
        buffer.Span[0] = false;
        return 1;
    }
    
    public ValueTask<int> DetourAsync(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken ct)
    {
        var path = context.GetArgument<string>(0);
        var state = LuaState.Create();
        return Handle();

        async ValueTask<int> Handle()
        {
            var results = await state.DoFileAccessAsync(path, cancellationToken: ct);
            if (results.Length > 0)
            {
                buffer.Span[0] = results[0];
                return 1;
            }

            return 0;
        }
    }
}
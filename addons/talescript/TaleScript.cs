using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Code.TaleScript.Extensions;
using Code.TaleScript.Runtime.Utils;
using Lua;
using Lua.Standard;

public partial class TaleScript : Node2D
{
    [Export] private RichTextLabel _richLabelText;
    private readonly TaskInterruptTokenSource _tits = new ();
    private readonly CancellationTokenSource _cts = new();

    private Dictionary<string, bool> stateflags = new();

    public override void _Ready()
    {
        Console.SetOut(new GodotConsoleWriter(GodotConsoleWriter.OutputType.Info));
        Console.SetError(new GodotConsoleWriter(GodotConsoleWriter.OutputType.Error));
        Test().Forget();
    }

    private async Task Test2()
    {
        var speed = 40f;
        await Effects.TypeWriterAsync(_richLabelText, speed, null, null, null, null, _tits, _cts.Token);
        await GDTask.WaitForInputEvent(nameof(Key.A));
        await Effects.TypeWriterAsync(_richLabelText, speed, null, null, null, null,  _tits, _cts.Token);
        await GDTask.WaitForInputEvent(nameof(MouseButton.Left));
        await Effects.TypeWriterAsync(_richLabelText, speed, null, null, null, null,  _tits, _cts.Token);
        await GDTask.WaitForInputEvent("ui_cancel");
        await Effects.TypeWriterAsync(_richLabelText, speed, null, null, null, null,  _tits, _cts.Token);
        await GDTask.WaitForInputEvent("ui_cancel");
        await Effects.TypeWriterAsync(_richLabelText, speed, null, null, null, null,  _tits, _cts.Token);
        await GDTask.WaitForInputEvent("ui_cancel");
        _richLabelText.Text = "done";
    }

    public override void _Input(InputEvent e)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            _tits.TryInterrupt();
        }
    }

    private async Task Test()
    {
        var state = CreateState();
        
        try
        {
            var results = await state.DoFileAccessAsync("Test.lua");
            if (results.Length != 0)
            {
                foreach (var result in results)
                {
                    if (result.TryRead<LuaTable>(out var table))
                    {
                        LuaValue current = LuaValue.Nil;
                        while (table.TryGetNext(current, out var pair))
                        {
                            current = pair.Key;
                            Console.WriteLine($"{pair.Key} = {pair.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Result: {result}");
                    }
                }
            }

            await GDTask.Delay(2f);
            var results2 = await state.DoFileAccessAsync("Test.lua");
        }
        catch (LuaRuntimeException e)
        {
            await Console.Error.WriteLineAsync($"LuaRuntimeException: {e}");
        }
        catch (LuaParseException e)
        {
            await Console.Error.WriteLineAsync($"LuaParseException: {e}");
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"Exception: {e}");
        }
    }

    private LuaState CreateState()
    {
        var state = LuaState.Create();
        
        state.Environment["detour"] =  new LuaFunction(async (context, buffer, ct) =>
        {
            var detourState = CreateState();
            var path = context.GetArgument<string>(0);
            var results = await detourState.DoFileAccessAsync(path, cancellationToken: ct);
            if (results.Length > 0)
            {
                buffer.Span[0] = results[0];
                return 1;
            }
            return 0;
        });
        
        state.Environment["once"] = new LuaFunction((context, buffer, ct) =>
        {
            var arg0 = context.GetArgument<string>(0);
            if(stateflags.TryGetValue(arg0, out var alreadyCalled) && alreadyCalled)
            {
                buffer.Span[0] = true;
                return new(1);
            }
            GD.Print(arg0);
            stateflags[arg0] = true;
            buffer.Span[0] = false;
            return new(1);
        });
        
        state.Environment["move"] = new LuaFunction((context, buffer, ct) =>
        {
            var arg0 = context.GetArgument<float>(0);
            var arg1 = context.GetArgument<float>(1);
            GlobalPosition  += new Vector2(arg0, arg1);
            return new(1);
        });
        
        state.Environment["wait"] = new LuaFunction(async (context, buffer, ct) =>
        {
            var sec = context.GetArgument<double>(0);
            await Task.Delay(TimeSpan.FromSeconds(sec), ct);
            return 0;
        });
        
        state.Environment["pprint"] = new LuaFunction((context, buffer, ct) =>
        {
            GD.Print(state);
            var sb = new System.Text.StringBuilder();
            sb.Append("Pprint:");
            if (context.ChunkName != null)
            {
                sb.Append($"From [{context.ChunkName}] ");
            }

            for (var index = 0; index < context.Arguments.Length; index++)
            {
                var value = context.Arguments[index];
                switch (value.Type)
                {
                    case LuaValueType.Nil:
                        if (index > 0) sb.Append(", ");
                        sb.Append("nil");
                        break;
                    case LuaValueType.Boolean:
                        if (index > 0) sb.Append(", ");
                        sb.Append(value.ToBoolean() ? "true" : "false");
                        break;
                    case LuaValueType.Number:
                        if (index > 0) sb.Append(", ");
                        sb.Append(value.ToString());
                        break;
                    case LuaValueType.String:
                        if (index > 0) sb.Append(", ");
                        sb.Append($"\"{value.ToString()}\"");
                        break;
                    case LuaValueType.Table:
                        var table = value.Read<LuaTable>();
                        if (index > 0) sb.Append(", ");
                        sb.Append("Table: { ");
                        var span = table.GetArraySpan();
                        for (var i = 0; i < span.Length; i++)
                        {
                            var pair = span[i];
                            if (pair.Type == LuaValueType.Nil)
                                continue;

                            if (i > 0) sb.Append(", ");
                            sb.Append($"{pair.ToString()}");
                        }

                        sb.Append(" }");
                        break;
                    default:
                        GD.Print(value.ToString());
                        break;
                }
            }

            GD.Print(sb.Length > 0 ? sb.ToString() : "nil");
            return new(1);
        });
        
        state.Environment["print_table"] = new LuaFunction((context, buffer, ct) =>
        {
            var table = context.GetArgument<LuaTable>(0);
            var values = table.GetArraySpan();

            var sb = new System.Text.StringBuilder();
            sb.Append("Table: { ");
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(values[i].ToString());
                if (i < values.Length - 1) sb.Append(", ");
            }
            sb.Append(" }");
            GD.Print(sb.ToString());

            return new(1);
        });
        state.OpenStandardLibraries();
        return state;
    }
    
    
}
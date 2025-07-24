using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Lua.Libraries;

public sealed class DebugLibrary
{
    public static readonly DebugLibrary Instance = new();

    public readonly LuaFunction[] Functions;
    
    public DebugLibrary()
    {
        Functions = [
            new("pprint", PrettyPrint),
        ];
    }

    public ValueTask<int> PrettyPrint(LuaFunctionExecutionContext context, Memory<LuaValue> buffer, CancellationToken ct)
    {
        var sb = new StringBuilder();
        sb.Append("Pprint: ");
        if (context.ChunkName != null)
            sb.Append($"From [{context.ChunkName}] ");

        for (var i = 0; i < context.Arguments.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            var value = context.Arguments[i];
            sb.Append(value.Type switch
            {
                LuaValueType.Nil when value.Type != LuaValueType.Table => "nil",
                LuaValueType.Boolean => value.ToBoolean() ? "true" : "false",
                LuaValueType.Number => value.ToString(),
                LuaValueType.String => $"\"{value}\"",
                LuaValueType.Table => FormatTable(value.Read<LuaTable>()),
                _ => value.ToString()
            });
        }
        
        GD.Print(sb.Length > 0 ? sb.ToString() : "nil");
        return new(0);
    }

    private static string FormatTable(LuaTable table)
    {
        var sb = new StringBuilder();
        var values = table.GetArraySpan();
        sb.Append("Table: { ");
        for (var i = 0; i < values.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(values[i].ToString());
        }
        sb.Append(" }");
        return sb.ToString();
    }
}
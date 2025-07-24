using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Lua;

namespace Code.TaleScript.Extensions;

public static class LuaStateExtensions
{
    private const string BASE_PATH = "res://addons/talescript/Lua/";
    public static ValueTask<LuaValue[]> DoFileAccessAsync(this LuaState state, string luaPath, CancellationToken cancellationToken = default)
    {
        var fullPath = BASE_PATH + luaPath;
        if (!FileAccess.FileExists(fullPath))
        {
            Godot.GD.PushError($"Lua file not found: {fullPath}");
            return new ValueTask<LuaValue[]>([]);
        }

        using var file = FileAccess.Open(fullPath, FileAccess.ModeFlags.Read);
        var luaCode = file.GetAsText();
        
        return state.DoStringAsync(luaCode, luaPath, cancellationToken: cancellationToken);
    }
}
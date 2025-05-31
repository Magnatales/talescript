using System.Threading;
using System.Threading.Tasks;
using Godot;
using Lua;

namespace Code.TaleScript.Extensions;

public static class LuaStateExtensions
{
    private const string BASE_PATH = "res://addons/talescript/Lua/";
    public static async ValueTask<LuaValue[]> DoFileAccessAsync(this LuaState state, string luaPath, CancellationToken cancellationToken = default)
    {
        var fullPath = BASE_PATH + luaPath;
        if (!FileAccess.FileExists(fullPath))
        {
            GD.PushError($"Lua file not found: {fullPath}");
            return [];
        }

        using var file = FileAccess.Open(fullPath, FileAccess.ModeFlags.Read);
        var luaCode = file.GetAsText();

        return await state.DoStringAsync(luaCode, "luaPath", cancellationToken: cancellationToken);
    }
}
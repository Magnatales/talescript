using System.Collections.Generic;
using System.Reflection;

namespace Code.TaleScript.Runtime;

public class TaleCommandRegistry
{
    private static readonly Dictionary<string, MethodInfo> _commands = new();

    public static void RegisterCommands(object target)
    {
        var type = target.GetType();
        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var attr = method.GetCustomAttribute<TaleScriptCommandAttribute>();
            if (attr != null)
            {
                _commands[attr.Name] = method;
            }
        }
    }

    public static bool Invoke(string commandName, object target, object[] args)
    {
        if (_commands.TryGetValue(commandName, out var method))
        {
            method.Invoke(target, args);
            return true;
        }
        return false;
    }
}
using System;

namespace Code.TaleScript;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class TaleScriptCommandAttribute : Attribute
{
    public string Name { get; }

    public TaleScriptCommandAttribute(string name)
    {
        Name = name;
    }
}
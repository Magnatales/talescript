using Godot;
using System;
using Code.TaleScript.Runtime.Utils;

public partial class InputGlobal : Node
{
    public override void _Input(InputEvent @event)
    {
        GDTask.OnInput(@event);
    }
}

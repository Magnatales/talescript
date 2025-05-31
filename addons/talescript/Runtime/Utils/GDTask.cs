using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Code.TaleScript.Runtime.Utils;

public static class GDTask
{
    private static readonly Dictionary<string, List<TaskCompletionSource>> _inputWaiters = new();
    
    public static async Task AsTask(this SignalAwaiter sa) => await sa;
    public static async Task WaitAsync(this SignalAwaiter sa, CancellationToken ct) => await sa.AsTask().WaitAsync(ct);
    
    public static async Task Delay(double duration, CancellationToken ct = default)
    {
        var sceneTree = (SceneTree)Engine.GetMainLoop();
        var timer = sceneTree.CreateTimer((float)duration);
        await sceneTree.ToSignal(timer, SceneTreeTimer.SignalName.Timeout).WaitAsync(ct);
    }
    
    public static async Task Yield(CancellationToken ct = default)
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame).WaitAsync(ct);
    }
    
    public static async Task WaitUntil(Func<bool> predicate, CancellationToken ct = default)
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        while (!predicate())
        {
            ct.ThrowIfCancellationRequested();
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }
    }

    public static async Task WaitWhile(Func<bool> predicate, CancellationToken ct = default)
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        while (predicate())
        {
            ct.ThrowIfCancellationRequested();
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        }
    }
    
    public static async Task WaitFrames(int frames, CancellationToken ct = default)
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        while (frames-- > 0 && !ct.IsCancellationRequested)
            await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
    }
    
    public static async Task WaitNextFrame(CancellationToken ct = default)
    {
        var tree = (SceneTree)Engine.GetMainLoop();
        await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        ct.ThrowIfCancellationRequested();
    }
    
    public static async void Forget(this Task task, CancellationToken ct = default)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) 
        {
           // Ignore cancellation, we are in a forget
        }
        catch (Exception ex)
        {
            GD.PushError($"[Forget] Unhandled exception: {ex}");
        }
    }
    
    public static async Task DelayInterruptable(float duration, TaskInterruptToken it = default, CancellationToken ct = default)
    {
        double timer = 0;
        while (timer < duration)
        {
            if (it.IsInterruptRequested)
            {
                return;
            }

            var mainTree = (SceneTree) Engine.GetMainLoop();
            var deltaTime = mainTree.Root.GetProcessDeltaTime();
            timer += deltaTime;
            await Delay(deltaTime, ct);
        }
    }
    
    public static Task WaitForInputEvent(string inputEventName, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource();

        if (!_inputWaiters.TryGetValue(inputEventName, out var list))
        {
            list = new List<TaskCompletionSource>();
            _inputWaiters[inputEventName] = list;
        }

        list.Add(tcs);

        if (ct.CanBeCanceled)
        {
            ct.Register(() =>
            {
                if (_inputWaiters.TryGetValue(inputEventName, out var l) && l.Remove(tcs))
                {
                    tcs.TrySetCanceled();
                    if (l.Count == 0)
                        _inputWaiters.Remove(inputEventName);
                }
            });
        }
        
        ct.ThrowIfCancellationRequested();

        return tcs.Task;
    }

    public static void OnInput(InputEvent e)
    {
        foreach (var action in _inputWaiters.Keys)
        {
            if (!InputMap.HasAction(action)) continue;
            if (!Input.IsActionJustPressed(action)) continue;
            GD.Print($"Action awaited pressed: {action}");
            if (_inputWaiters.TryGetValue(action, out var list))
            {
                foreach (var tcs in list)
                    tcs.TrySetResult();

                _inputWaiters.Remove(action);
            }
            return;

        }

        switch (e)
        {
            case InputEventKey eventKey when eventKey.IsPressed():
            {
                var keyString = eventKey.Keycode.ToString();

                if (_inputWaiters.TryGetValue(keyString, out var list))
                {
                    GD.Print($"Key awaited pressed: {keyString}");
                    foreach (var tcs in list)
                        tcs.TrySetResult();

                    _inputWaiters.Remove(keyString);
                }

                return;
            }
            case InputEventMouseButton eventMouseButton when eventMouseButton.IsPressed():
            {
                var buttonString = eventMouseButton.ButtonIndex.ToString();

                if (_inputWaiters.TryGetValue(buttonString, out var list))
                {
                    GD.Print($"Mouse button awaited pressed: {buttonString}");
                    foreach (var tcs in list)
                        tcs.TrySetResult();

                    _inputWaiters.Remove(buttonString);
                }

                break;
            }
        }
    }
}
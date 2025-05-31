using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace Code.TaleScript.Runtime.Utils;

public static class Effects
{
    public static async Task FadeAlpha(Control control, float from, float to, float fadeTime, CancellationToken ct = default)
    {
        var mainTree = (SceneTree)Engine.GetMainLoop();

        var color = control.Modulate;
        color.A = from;
        control.Modulate = color;

        var destinationColor = color;
        destinationColor.A = to;

        var tween = control.CreateTween();
        tween.TweenProperty(control, "modulate", destinationColor, fadeTime);
        while (tween.IsRunning())
        {
            if (!GodotObject.IsInstanceValid(control))
            {
                // the control was deleted from the scene
                return;
            }

            if (ct.IsCancellationRequested)
            {
                tween.Kill();
                return;
            }

            await GDTask.Delay(mainTree.Root.GetProcessDeltaTime(), ct);
        }

        color.A = to;
        if (color.A >= 1f)
        {
            control.Visible = true;
        }

        control.Modulate = color;
    }

    public static async Task TypeWriterAsync(
        RichTextLabel text,
        float lettersPerSecond,
        Action<int> onCharacterTyped = null,
        Action onPauseStarted = null,
        Action onPauseEnded = null,
        Stack<(int position, float duration)> pausePositions = null,
        TaskInterruptTokenSource tits = null,
        CancellationToken ct = default)
    {
        var mainTree = (SceneTree)Engine.GetMainLoop();
        tits?.Start();
        text.VisibleCharacters = 0;
        text.VisibleRatio = 0;

        await GDTask.Yield(ct);

        var characterCount = text.GetTotalCharacterCount();

        if (tits?.Token.IsInterruptRequested ?? false)
        {
            text.VisibleRatio = 1f;
            return;
        }
        
        //Early out if letter speed is 0 or if there are no characters
        if (lettersPerSecond <= 0 || characterCount == 0)
        {
            // Show everything and return
            text.VisibleRatio = 1;
            tits?.Complete();
            return;
        }

        var secondsPerLetter = 1.0f / lettersPerSecond;
        var deltaTime = mainTree.Root.GetProcessDeltaTime();
        var accumulator = deltaTime;

        while (GodotObject.IsInstanceValid(text) && text.VisibleCharacters < characterCount)
        {
            if (tits != null && tits.Token.IsInterruptRequested)
            {
                text.VisibleRatio = 1f;
                return;
            }
            
            // pause logic
            if (pausePositions is { Count: > 0 } &&
                pausePositions.Peek().position == text.VisibleCharacters)
            {
                var pause = pausePositions.Pop();
                onPauseStarted?.Invoke();
                await GDTask.DelayInterruptable(pause.duration, tits?.Token ?? default , ct);
                onPauseEnded?.Invoke();
                accumulator = 0;
                continue;
            }

            //  character appearance
            while (accumulator >= secondsPerLetter && text.VisibleCharacters < characterCount)
            {
                text.VisibleCharacters++;

                onCharacterTyped?.Invoke(text.VisibleCharacters - 1);

                accumulator -= secondsPerLetter;
                
                char currentChar = GetCharAt(text.Text, text.VisibleCharacters - 1);
                
                var pauseDuration = currentChar switch
                {
                    '.' or '!' or '?' => 0.5f,
                    ',' or ';' or ':' => 0.25f,
                    '\n'              => 0.4f,
                    '…'               => 0.7f,
                    _                 => 0f
                };

                if (pauseDuration > 0)
                {
                    await GDTask.DelayInterruptable(pauseDuration, tits?.Token ?? default, ct);
                }
            }

            accumulator += deltaTime;
            await GDTask.Delay(deltaTime, ct);
        }

        if (GodotObject.IsInstanceValid(text))
        {
           text.VisibleCharacters = characterCount;
           text.VisibleRatio = 1f;
        }
        tits?.Complete();
    }
    
    private static char GetCharAt(string input, int index)
    {
        if (index >= 0 && index < input.Length)
            return input[index];
        return ' ';
    }
}
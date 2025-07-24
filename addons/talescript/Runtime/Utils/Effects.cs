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

            await GDTask.Delay((float)mainTree.Root.GetProcessDeltaTime(), ct);
        }

        control.Modulate = color;
    }
    
    public static async Task TypeWriterAsync(RichTextLabel textLabel, string text, float lettersPerSecond, InterruptTokenSource it = null, CancellationToken ct = default)
    {
        textLabel.Text = text;
        await TypeWriterAsync(textLabel, lettersPerSecond, null, null, null, null, it, ct);
    }

    public static async Task TypeWriterAsync(RichTextLabel textLabel, float lettersPerSecond, InterruptTokenSource it = null, CancellationToken ct = default)
    {
        await TypeWriterAsync(textLabel, lettersPerSecond, null, null, null, null, it, ct);
    }

    public static async Task TypeWriterAsync(
        RichTextLabel textLabel,
        float lettersPerSecond,
        Action<int> onCharacterTyped = null,
        Action onPauseStarted = null,
        Action onPauseEnded = null,
        Stack<(int position, float duration)> pausePositions = null,
        InterruptTokenSource its = null,
        CancellationToken ct = default)
    {
        var mainTree = (SceneTree)Engine.GetMainLoop();
        its?.Start();
        textLabel.VisibleCharacters = 0;
        textLabel.VisibleRatio = 0;

        var characterCount = textLabel.GetTotalCharacterCount();

        if (its?.Token.IsInterruptRequested ?? false)
        {
            textLabel.VisibleRatio = 1f;
            return;
        }
        
        //Early out if letter speed is 0 or if there are no characters
        if (lettersPerSecond <= 0 || characterCount == 0)
        {
            // Show everything and return
            textLabel.VisibleRatio = 1;
            its?.Complete();
            return;
        }

        var secondsPerLetter = 1.0f / lettersPerSecond;
        var deltaTime = (float)mainTree.Root.GetProcessDeltaTime();
        var accumulator = deltaTime;

        while (GodotObject.IsInstanceValid(textLabel) && textLabel.VisibleCharacters < characterCount)
        {
            if (its != null && its.Token.IsInterruptRequested)
            {
                textLabel.VisibleRatio = 1f;
                return;
            }
            
            // pause logic
            if (pausePositions is { Count: > 0 } &&
                pausePositions.Peek().position == textLabel.VisibleCharacters)
            {
                var pause = pausePositions.Pop();
                onPauseStarted?.Invoke();
                await GDTask.DelayInterruptable(pause.duration, its?.Token ?? default , ct);
                onPauseEnded?.Invoke();
                accumulator = 0;
                continue;
            }

            //  character appearance
            while (accumulator >= secondsPerLetter && textLabel.VisibleCharacters < characterCount)
            {
                textLabel.VisibleCharacters++;

                onCharacterTyped?.Invoke(textLabel.VisibleCharacters - 1);

                accumulator -= secondsPerLetter;
                
                var currentChar = GetCharAt(textLabel.Text, textLabel.VisibleCharacters - 1);
                
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
                    await GDTask.DelayInterruptable(pauseDuration, its?.Token ?? default, ct);
                }
            }

            accumulator += deltaTime;
            await GDTask.Delay(deltaTime, ct);
        }

        if (GodotObject.IsInstanceValid(textLabel))
        {
           textLabel.VisibleCharacters = characterCount;
           textLabel.VisibleRatio = 1f;
        }
        its?.Complete();
    }
    
    private static char GetCharAt(string input, int index)
    {
        if (index >= 0 && index < input.Length)
            return input[index];
        return ' ';
    }
}
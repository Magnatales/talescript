using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Code.TaleScript.Extensions;
using Code.TaleScript.Runtime.Utils;
using Lua;
using Lua.Libraries;
using Lua.Standard;

public partial class TaleScript : Node2D
{
    [Export] private RichTextLabel _richLabelText;
    private readonly InterruptTokenSource _its = new ();
    private CancellationTokenSource _cts = new();
    
    private Queue<Story> _storyCache = new();
    private bool _isPreloading = false;
    private const int CacheSize = 10;
    private int _shown;

    public override void _Ready()
    {
        _richLabelText.Text = "Fetching stories...";
        PreloadStoriesAsync().Forget();
        Test2().Forget();
    }
    
    private async Task PreloadStoriesAsync()
    {
        if (_isPreloading) return;
        _isPreloading = true;

        try
        {
            for (int i = 0; i < CacheSize; i++)
            {
                var story = await StoryFetcher.GetRandomStoryAsync();
                _storyCache.Enqueue(story);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr("Preload failed: ", e.Message);
        }
        finally
        {
            _isPreloading = false;
        }
    }
    
    private async Task<Story> GetStoryAsync()
    {
        if (_storyCache.Count > 0)
        {
            return _storyCache.Dequeue();
        }

        // No cache — fetch one immediately
        var story = await StoryFetcher.GetRandomStoryAsync();

        // Start preloading the rest in the background
        if (!_isPreloading)
            _ = PreloadStoriesAsync();

        return story;
    }

    private async Task Test2()
    {
        for (int i = 0; i < CacheSize + 1; i++)
        {
            await TestText();
            _richLabelText.Text += "\n\nClick to continue...";
            await GDTask.WaitForInputEvent(nameof(MouseButton.Left));
        }
        _richLabelText.Text = "done";
    }

    private async Task TestText()
    {
        // Check duration with a stopwatch
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var story = await GetStoryAsync();
        var text = story.StoryText;
        _shown++;
        _richLabelText.Text = $"{_shown}/{CacheSize + 1} "; 
        _richLabelText.Text += text;
        stopwatch.Stop();
        //Debug in seconds
        GD.Print($"Fetched story in {stopwatch.Elapsed.TotalSeconds} seconds");
        await Effects.TypeWriterAsync(_richLabelText, 40f, _its, _cts.Token);
    }

    public override void _Input(InputEvent e)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            _its.TryInterrupt();
        }
    }

    private async Task Test()
    {
        var state = CreateState();
        try
        {
            var results = await state.DoFileAccessAsync("Test.lua", cancellationToken: _cts.Token);
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
        state.OpenBasicLibrary();
        state.OpenDialogLibrary();
        state.OpenDebugLibrary();
        state.OpenActorLibrary();
        state.OpenAsyncLibrary();
        return state;
    }

}

public class Story
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string StoryText { get; set; }
    public string Moral { get; set; }
}

public static class StoryFetcher
{
    private static readonly System.Net.Http.HttpClient httpClient = new ();

    public static async Task<Story> GetRandomStoryAsync()
    {
        var response = await httpClient.GetAsync("https://shortstories-api.onrender.com/");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return new Story
        {
            Title = root.GetProperty("title").GetString(),
            Author = root.GetProperty("author").GetString(),
            StoryText = root.GetProperty("story").GetString(),
            Moral = root.GetProperty("moral").GetString()
        };
    }
}

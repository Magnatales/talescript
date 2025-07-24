using System.Diagnostics;
using Godot;

public static class LogUtils
{
    public static void PrintWithTrace(string message = "")
    {
        var stackTrace = new StackTrace(true);
        var frame = stackTrace.GetFrame(1); // caller
        var file = frame.GetFileName();
        var line = frame.GetFileLineNumber();

        if (file != null)
        {
            // Convert to Godot-style path if needed
            var godotPath = file.Replace(System.IO.Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar, "res://").Replace("\\", "/");
            GD.Print($"{message}  {godotPath}:{line}");
        }
        else
        {
            GD.Print(message);
        }
    }
}
using System.Text;
using Godot;

namespace Code.TaleScript.Runtime.Utils;

public class GodotConsoleWriter : System.IO.TextWriter
{
    
    public enum OutputType
    {
        Info,
        Warning,
        Error
    }

    private StringBuilder buffer = new StringBuilder();
    private readonly OutputType type;

    public GodotConsoleWriter(OutputType outputType)
    {
        this.type = outputType;
    }

    public override void Write(char value)
    {
        if (value == '\n')
        {
            FlushBuffer();
        }
        else if (value != '\r')
        {
            buffer.Append(value);
        }
    }

    public override void Write(string value)
    {
        foreach (char c in value)
        {
            Write(c);
        }
    }

    private void FlushBuffer()
    {
        if (buffer.Length == 0)
            return;

        switch (type)
        {
            case OutputType.Info:
                GD.Print(buffer.ToString());
                break;
            case OutputType.Warning:
                GD.PushWarning(buffer.ToString());
                break;
            case OutputType.Error:
                GD.PushError(buffer.ToString());
                break;
        }

        buffer.Clear();
    }

    public override void Flush()
    {
        FlushBuffer();
        base.Flush();
    }

    public override Encoding Encoding => Encoding.UTF8;
}
using System;

public class TaskInterruptTokenSource
{
    internal enum TokenState
    {
        NotRunning,
        Running,
        Interrupted,
    }

    internal TokenState State { get; private set; } = TokenState.NotRunning;

    public TaskInterruptToken Token => new(this);

    public void Start() => State = TokenState.Running;
    
    public void TryInterrupt()
    {
        if (State == TokenState.Running)
            State = TokenState.Interrupted;
    }

    public void Complete() => State = TokenState.NotRunning;
    public bool CanBeInterupted => State == TokenState.Running;
}

public readonly struct TaskInterruptToken
{
    private readonly TaskInterruptTokenSource _source;

    internal TaskInterruptToken(TaskInterruptTokenSource source)
    {
        _source = source;
    }

    public bool IsInterruptRequested => _source?.State == TaskInterruptTokenSource.TokenState.Interrupted;
}


using System;

public class InterruptTokenSource
{
    internal enum TokenState
    {
        NotRunning,
        Running,
        Interrupted,
    }

    internal TokenState State { get; private set; } = TokenState.NotRunning;

    public InterruptToken Token => new(this);

    public void Start() => State = TokenState.Running;
    
    public void TryInterrupt()
    {
        if (State == TokenState.Running)
            State = TokenState.Interrupted;
    }

    public void Complete() => State = TokenState.NotRunning;
}

public readonly struct InterruptToken
{
    private readonly InterruptTokenSource _source;

    internal InterruptToken(InterruptTokenSource source)
    {
        _source = source;
    }

    public bool IsInterruptRequested => _source?.State == InterruptTokenSource.TokenState.Interrupted;
}


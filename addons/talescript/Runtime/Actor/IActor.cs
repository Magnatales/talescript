namespace Code.TaleScript.Runtime.Actor;

public interface IActor
{
    string Name { get; }
    void Move(float x, float y);
    void Teleport(float x, float y);
    void PlayAnimation(string animationName);
    void StopAnimation();
}
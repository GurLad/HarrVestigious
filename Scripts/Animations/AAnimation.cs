using Godot;
using System;

public abstract class AAnimation : Node
{
    [Export]
    public float Time;
    public bool Done { get => timer.TimeLeft <= 0; }
    private Timer timer;

    public override void _Ready()
    {
        base._Ready();
        timer = new Timer();
        timer.WaitTime = Time;
        timer.OneShot = true;
        AddChild(timer);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (!Done)
        {
            AnimateFrame(1 - timer.TimeLeft / timer.WaitTime);
        }
    }

    public virtual AAnimation Begin(Spatial target, AAnimationArgs args)
    {
        timer.Start();
        return this; // For ease-of-use in unit
    }

    protected abstract void AnimateFrame(float percent);
}

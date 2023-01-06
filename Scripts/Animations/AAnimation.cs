using Godot;
using System;

public abstract class AAnimation : Node
{
    public bool Done { get => timer.TimeLeft <= 0; }
    protected Timer timer;
    protected Spatial target;

    public override void _Ready()
    {
        base._Ready();
        timer = new Timer();
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

    public abstract AAnimation Begin(Spatial target, AAnimationArgs args);

    public abstract void AnimateFrame(float percent);
}


public abstract class AAnimation<T> : AAnimation where T : AAnimationArgs
{
    protected T args;

    public override AAnimation Begin(Spatial target, AAnimationArgs args)
    {
        if (!(args is T))
        {
            throw new Exception("Invalid args format!");
        }
        this.args = (T)args;
        this.target = target;
        timer.WaitTime = args.Time;
        timer.Start();
        return this; // For ease-of-use in unit
    }
}
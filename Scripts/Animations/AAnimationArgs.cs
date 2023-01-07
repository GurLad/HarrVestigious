using Godot;
using System;

public abstract class AAnimationArgs
{
    public float Time;

    protected AAnimationArgs(float time)
    {
        Time = time;
    }
}

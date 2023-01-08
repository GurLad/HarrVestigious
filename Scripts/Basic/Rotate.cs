using Godot;
using System;

public class Rotate : Particles
{
    [Export]
    public float Speed;

    public override void _Process(float delta)
    {
        base._Process(delta);
        RotateY(Speed * delta);
    }
}

using Godot;
using System;

public class LavaObject : MeshInstance
{
    [Export]
    public float TimeMod;
    [Export]
    public float OffsetMod;
    [Export]
    public float SizeMod;
    private Transform baseTransform;
    private float offset;

    public override void _Ready()
    {
        base._Ready();
        baseTransform = new Transform(Transform.basis, Transform.origin);
        offset = GlobalTranslation.x + GlobalTranslation.z;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        Transform = baseTransform.Translated(new Vector3(0, Mathf.Sin(Time.GetTicksMsec() * TimeMod + offset * OffsetMod) * SizeMod, 0));
    }
}

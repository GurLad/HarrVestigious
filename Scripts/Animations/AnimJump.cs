using Godot;
using System;

public class AnimJump : AAnimation<AnimJump.Args>
{
    private Transform baseTransform;

    public override AAnimation Begin(Spatial target, AAnimationArgs args)
    {
        baseTransform = target.Transform;
        return base.Begin(target, args);
    }

    public override void AnimateFrame(float percent)
    {
        Vector3 offset = args.Direction.To3D() * percent + new Vector3(0, percent * args.Height - percent * percent * args.Height, 0);
        target.Transform = baseTransform.Translated(offset);
    }

    public class Args : AAnimationArgs
    {
        public float Height;
        public Vector2Int Direction;

        public Args(float time, float height, Vector2Int direction) : base(time)
        {
            Height = height;
            Direction = direction;
        }
    }
}

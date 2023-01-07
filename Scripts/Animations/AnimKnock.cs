using Godot;
using System;

public class AnimKnock : AAnimation<AnimKnock.Args>
{
    private Transform baseTransform;

    public override AAnimation Begin(Spatial target, AAnimationArgs args)
    {
        baseTransform = target.Transform;
        return base.Begin(target, args);
    }

    public override void AnimateFrame(float percent)
    {
        if (!args.OneWay)
        {
            percent *= 2;
        }
        float percentEased = (percent <= 1 ^ args.Back) ? Easing.EaseInQuart(percent) : Easing.EaseOutQuart(2 - percent);
        Vector3 rotationAxis = args.Direction.To3D().Normalized();
        target.Transform = target.Transform.Rotated(rotationAxis, args.Degree * percentEased);
    }

    public class Args : AAnimationArgs
    {
        public float Degree;
        public Vector2Int Direction;
        public bool OneWay;
        public bool Back;

        public Args(float time, float degree, Vector2Int direction, bool oneWay, bool back) : base(time)
        {
            Degree = degree;
            Direction = direction;
            OneWay = oneWay;
            Back = back;
        }
    }
}

using Godot;
using System;

public class AnimSquash : AAnimation<AnimSquash.Args>
{
    private Vector3 baseScale;

    public override AAnimation Begin(Spatial target, AAnimationArgs args)
    {
        baseScale = target.Scale;
        return base.Begin(target, args);
    }

    public override void AnimateFrame(float percent)
    {
        if (!args.OneWay)
        {
            percent *= 2;
        }
        Vector3 scaleFullSquash = (Vector3.One - args.Direction.Normalized() * args.Strength) * baseScale;
        float percentEased = Easing.EaseOutQuart(percent <= 1 ? percent : (2 - percent));
        target.Scale = scaleFullSquash * percentEased + baseScale * (1 - percentEased);
    }

    public class Args : AAnimationArgs
    {
        public float Strength;
        public Vector3 Direction;
        public bool OneWay;

        public Args(float time, float strength, Vector3 direction, bool oneWay)
        {
            Time = time;
            Strength = strength;
            Direction = direction;
            OneWay = oneWay;
        }
    }
}

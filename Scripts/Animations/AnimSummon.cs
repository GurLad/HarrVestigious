using Godot;
using System;

public class AnimSummon : AAnimation<AnimSummon.Args>
{
    private Vector3 baseScale => args.Scale;

    public override AAnimation Begin(Spatial target, AAnimationArgs args)
    {
        return base.Begin(target, args);
    }

    public override void AnimateFrame(float percent)
    {
        float percentEased = Easing.EaseInBack(percent <= 1 ? percent : (2 - percent));
        target.Scale = baseScale * percentEased;
    }

    public class Args : AAnimationArgs
    {
        public Vector3 Scale;

        public Args(float time, Vector3 scale) : base(time) { Scale = scale; }
    }
}

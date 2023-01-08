using Godot;
using System;

public class AnimDie : AAnimation<AnimDie.Args>
{
    private Vector3 baseScale;

    public override AAnimation Begin(Spatial target, AAnimationArgs args)
    {
        baseScale = target.Scale;
        return base.Begin(target, args);
    }

    public override void AnimateFrame(float percent)
    {
        float percentEased = Easing.EaseInBack(percent <= 1 ? percent : (2 - percent));
        target.Scale = baseScale * (1 - percentEased);
    }

    public class Args : AAnimationArgs
    {
        public Args(float time) : base(time) { }
    }
}

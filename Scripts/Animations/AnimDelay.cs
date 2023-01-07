using Godot;
using System;

public class AnimDelay : AAnimation<AnimDelay.Args>
{
    public override void AnimateFrame(float percent)
    {
        // Do nothing
    }

    public class Args : AAnimationArgs
    {
        public Args(float time) : base(time) { }
    }
}

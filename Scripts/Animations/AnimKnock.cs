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
        float percentEased;
        if (!args.Back)
        {
            percentEased = (percent <= 1) ? Easing.EaseInQuart(percent) : Easing.EaseOutQuart(2 - percent);
        }
        else
        {
            percentEased = (percent <= 1) ? Easing.EaseOutQuart(percent) : Easing.EaseInQuart(2 - percent);
        }
        Vector3 rotationAxis = new Vector2Int(-args.Direction.y, args.Direction.x).To3D().Normalized();
        Transform temp = new Transform(baseTransform.basis, baseTransform.origin);
        temp.basis = temp.basis.Rotated(rotationAxis, args.Degree * percentEased);
        target.Transform = temp;
    }

    public class Args : AAnimationArgs
    {
        public float Degree;
        public Vector2Int Direction;
        public bool OneWay;
        public bool Back;

        public Args(float time, float degree, Vector2Int direction, bool oneWay, bool back) : base(time)
        {
            Degree = degree * Mathf.Pi / 180;
            Direction = direction;
            OneWay = oneWay;
            Back = back;
        }
    }
}

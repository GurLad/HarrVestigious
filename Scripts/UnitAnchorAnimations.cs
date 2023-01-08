using Godot;
using System;

public class UnitAnchorAnimations : Spatial
{
    public enum Mode { None = 0, Breath = 1, Shake = 2, Flying = 4, Vest = 8 }
    // Breath
    [Export]
    public Vector2 BreathSizeRange;
    [Export]
    public Vector2 BreathTimeRange;
    private Timer breathTimer;
    private Vector3 breathBaseScale;
    private Vector3 breathSquashedScale;
    // Shake TBA
    [Export]
    public Vector2 ShakeSizeRange;
    [Export]
    public Vector2 ShakeTimeRange;
    private Timer shakerTimer;
    private Transform shakeBaseTransform;
    private Vector3 shakeOffset;
    // Flying TBA
    // Misc
    public Mode CurrentMode;
    private Random rng = new Random();

    public override void _Ready()
    {
        base._Ready();
        // Breath
        breathTimer = new Timer();
        breathTimer.OneShot = true;
        breathBaseScale = Scale;
        AddChild(breathTimer);
        // Shake
        shakerTimer = new Timer();
        shakerTimer.OneShot = true;
        shakeBaseTransform = Transform;
        AddChild(shakerTimer);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        // Breath
        if ((CurrentMode & Mode.Breath) != Mode.None)
        {
            float percent = breathTimer.Percent() * 2;
            float percentEased = Easing.EaseInOutQuad(percent <= 1 ? percent : (2 - percent));
            Scale = breathSquashedScale * percentEased + breathBaseScale * (1 - percentEased);
            if (breathTimer.Percent() >= 1)
            {
                BreathRandomizeValues();
                breathTimer.Start();
            }
        }
        else
        {
            breathTimer.Stop();
            Scale = breathBaseScale;
        }
        // Shake
        if ((CurrentMode & Mode.Shake) != Mode.None)
        {
            float percent = shakerTimer.Percent() * 4;
            //int signMode = percent > 2 ? -1 : 1;
            //percent = percent % 2;
            float percentEased = Easing.EaseInOutSin(percent);
            Vector3 offset = shakeOffset * percentEased;
            Transform = shakeBaseTransform.Translated(offset);
            if (shakerTimer.Percent() >= 1)
            {
                ShakeRandomizeValues();
                shakerTimer.Start();
            }
        }
        else
        {
            shakerTimer.Stop();
            Transform = shakeBaseTransform;
        }
    }

    public void AddAnimation(Mode mode)
    {
        CurrentMode |= mode;
        switch (mode)
        {
            case Mode.None:
                break;
            case Mode.Breath:
                BreathRandomizeValues();
                breathTimer.Start(rng.NextFloat(0, breathTimer.WaitTime));
                break;
            case Mode.Shake:
                ShakeRandomizeValues();
                shakerTimer.Start(rng.NextFloat(0, shakerTimer.WaitTime));
                break;
            case Mode.Flying:
                break;
            default:
                break;
        }
    }

    public void RemoveAnimation(Mode mode)
    {
        CurrentMode &= ~mode;
    }

    private void BreathRandomizeValues()
    {
        breathTimer.WaitTime = rng.NextFloat(BreathTimeRange);
        breathSquashedScale = (Vector3.One - new Vector3(0, 1, 0) * rng.NextFloat(BreathSizeRange)) * breathBaseScale;
    }

    private void ShakeRandomizeValues()
    {
        shakerTimer.WaitTime = rng.NextFloat(ShakeTimeRange);
        shakeOffset = new Vector3(1, 0, 0) * rng.NextFloat(ShakeSizeRange);
    }
}

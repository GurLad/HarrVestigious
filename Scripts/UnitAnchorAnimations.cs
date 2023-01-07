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
    // Flying TBA
    // Misc
    public Mode CurrentMode;
    private Random rng = new Random();

    public override void _Ready()
    {
        base._Ready();
        breathTimer = new Timer();
        breathTimer.OneShot = true;
        breathBaseScale = Scale;
        AddChild(breathTimer);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
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
}

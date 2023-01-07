using Godot;
using System;

public class UnitPanel : Panel
{
    private enum State { None, Focusing, Unfocusing }
    [Export]
    public float InactiveSize;
    [Export]
    public float ActiveSize;
    [Export]
    public Texture VestTexture;
    [Export]
    public Texture FreeTexture;
    public Unit Unit;
    public Action OnDone;
    private TextureRect unitIcon;
    private TextureRect vestIcon;
    private Timer timer;
    private State state;
    private Vector2 baseSize;

    public override void _Process(float delta)
    {
        base._Process(delta);
        float percent;
        switch (state)
        {
            case State.None:
                break;
            case State.Focusing:
                percent = Easing.EaseOutBack(timer.Percent());
                RectSize = new Vector2(percent * ActiveSize + (1 - percent) * InactiveSize, baseSize.y);
                if (timer.TimeLeft <= 0)
                {
                    state = State.None;
                    OnDone?.Invoke();
                }
                break;
            case State.Unfocusing:
                percent = Easing.EaseInBack(timer.Percent());
                RectSize = new Vector2(percent * InactiveSize + (1 - percent) * ActiveSize, baseSize.y);
                if (timer.TimeLeft <= 0)
                {
                    state = State.None;
                    OnDone?.Invoke();
                }
                break;
            default:
                break;
        }
    }

    public void Init()
    {
        unitIcon = GetNode<TextureRect>("HBoxContainer/UnitIcon");
        vestIcon = GetNode<TextureRect>("HBoxContainer/VestIcon");
        timer = GetNode<Timer>("AnimationDelay");
        RectSize = new Vector2(InactiveSize, RectSize.y);
        baseSize = RectSize;
    }

    public void UpdateUnitData(Unit unit)
    {
        Unit = unit;
        unitIcon.Texture = unit.Icon;
        vestIcon.Texture = unit.HasVest ? VestTexture : FreeTexture;
    }

    public void Activate(Action onDone = null)
    {
        if (state != State.None)
        {
            throw new Exception("Slow down!");
        }
        OnDone = onDone;
        timer.Start();
        state = State.Focusing;
    }

    public void Deactivate(Action onDone = null)
    {
        if (state != State.None)
        {
            throw new Exception("Slow down!");
        }
        OnDone = onDone;
        timer.Start();
        state = State.Unfocusing;
    }
}

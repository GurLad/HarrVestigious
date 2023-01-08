using Godot;
using System;
using System.Collections.Generic;

public class UnitPanel : Panel
{
    private enum State { None, Focusing, Unfocusing }
    [Export]
    public float InactiveSize;
    [Export]
    public float ActiveSize;
    [Export]
    public Color MarkedColor;
    [Export]
    public Color NormalColor;
    [Export]
    public Texture VestTexture;
    [Export]
    public Texture FreeTexture;
    [Export]
    public PackedScene DoubleStatContainer;
    [Export]
    public Texture[] StatTextures;
    public Unit Unit;
    public Action OnDone;
    private TextureRect unitIcon;
    private TextureRect vestIcon;
    private Timer timer;
    private State state;
    private Vector2 baseSize;
    private List<Label> statLabels = new List<Label>();

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
        // Generate stat labels
        HBoxContainer container = GetNode<HBoxContainer>("HBoxContainer");
        for (int i = 0; i < Unit.STAT_COUNT / 2; i++)
        {
            Control doubleStatContainer = DoubleStatContainer.Instance<Control>();
            for (int j = 0; j < 2; j++)
            {
                doubleStatContainer.GetNode<TextureRect>("Container" + (j + 1) + "/StatIcon").Texture = StatTextures[i * 2 + j];
                statLabels.Add(doubleStatContainer.GetNode<Label>("Container" + (j + 1) + "/StatLabel"));
            }
            container.AddChild(doubleStatContainer);
        }
    }

    public void UpdateUnitData(Unit unit)
    {
        Unit = unit;
        unitIcon.Texture = unit.Icon;
        vestIcon.Texture = unit.HasVest ? VestTexture : FreeTexture;
        for (int i = 0; i < Unit.STAT_COUNT; i++)
        {
            statLabels[i].Text = unit.GetStatString(i);
        }
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

    public void Mark()
    {
        Modulate = new Color(MarkedColor, Modulate.a);
    }

    public void Unmark()
    {
        Modulate = new Color(NormalColor, Modulate.a);
    }

    public void _OnMouseEntered()
    {
        Modulate = new Color(Modulate, 0.2f);
    }

    public void _OnMouseLeave()
    {
        Modulate = new Color(Modulate, 1);
    }
}

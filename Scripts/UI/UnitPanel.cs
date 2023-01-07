using Godot;
using System;

public class UnitPanel : Panel
{
    [Export]
    public float InactiveSize;
    [Export]
    public float ActiveSize;
    private TextureRect UnitIcon;
    private TextureRect VestIcon;

    public void UpdateUnitData(Unit unit)
    {

    }
}

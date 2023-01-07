using Godot;
using System;

public abstract class AUnitAction
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Vector2Int Range { get; }
    public abstract bool RequiresTarget { get; }
    public abstract bool FreeAction { get; }
    protected Unit thisUnit;

    public abstract void Activate(Vector2Int target = null);
    public abstract void PassiveEffect();

    public void AttachToUnit(Unit unit)
    {
        thisUnit = unit;
    }
}

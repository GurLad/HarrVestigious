using Godot;
using System;

public abstract class AUnitAction
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Vector2Int Range { get; }
    public abstract bool RequiresTarget { get; }
    public abstract bool FreeAction { get; }
    public abstract bool UseMoveMarkers { get; }
    public bool Exhausted;
    protected Unit thisUnit;

    protected abstract void ActivateEffect(Vector2Int target = null);
    public abstract void PassiveEffect();
    public abstract bool ValidTarget(Unit target);

    public void AttachToUnit(Unit unit)
    {
        thisUnit = unit;
    }

    public void Activate(Vector2Int target = null)
    {
        if (RequiresTarget && target == null)
        {
            throw new Exception(Name + " requires a target!");
        }
        thisUnit.PlayerUIController.HideUI(false);
        ActivateEffect(target);
        if (!FreeAction)
        {
            thisUnit.QueueImmediateAction(() => thisUnit.EndTurn());
        }
        else
        {
            Exhausted = true;
            thisUnit.QueueImmediateAction(() => thisUnit.BeginTurn(false));
        }
    }
}

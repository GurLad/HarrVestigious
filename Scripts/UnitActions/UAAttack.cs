using Godot;
using System;

public class UAAttack : AUnitAction
{
    public override string Name => "Attack";
    public override string Description => "Attacks a target.";
    public override Vector2Int Range => new Vector2Int(thisUnit.AttackRange);
    public override bool RequiresTarget => true;
    public override bool FreeAction => false;
    public override bool UseMoveMarkers => false;
    public override int SortOrder => 2;

    protected override void ActivateEffect(Vector2Int target = null)
    {
        Unit targetUnit = thisUnit.TurnFlowController.GetUnitAtPos(target);
        if (targetUnit != null)
        {
            thisUnit.QueueImmediateAction(() => thisUnit.PlaySFX(Unit.SFXType.Attack));
            thisUnit.QueueAnimation(new AnimKnock(), new AnimKnock.Args(0.3f, 30, targetUnit.Pos - thisUnit.Pos, true, false));
            thisUnit.QueueImmediateAction(() =>
            {
                targetUnit.QueueAnimation(new AnimKnock(), new AnimKnock.Args(0.8f, 50, targetUnit.Pos - thisUnit.Pos, false, true));
                targetUnit.TakeDamage(Mathf.Max(0, thisUnit.Attack - targetUnit.Defense), thisUnit);
            });
            thisUnit.QueueAnimation(new AnimKnock(), new AnimKnock.Args(0.3f, -30, targetUnit.Pos - thisUnit.Pos, true, false));
            bool targetDies = targetUnit.Health - thisUnit.Attack + targetUnit.Defense <= 0;
            thisUnit.QueueAnimation(new AnimDelay(), new AnimDelay.Args(targetDies ? 1.25f : 0.5f));
        }
        else
        {
            throw new Exception("Literally impossible");
        }
    }

    public override void PassiveEffect()
    {
        // Do nothing
    }

    public override bool ValidTarget(Unit target)
    {
        return true;
    }
}

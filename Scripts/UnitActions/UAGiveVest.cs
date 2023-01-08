using Godot;
using System;

public class UAGiveVest : AUnitAction
{
    public override string Name => "Transfer Vest";
    public override string Description => "Gives the Cursed Vest to a target, granting you control over it.\nStuns the current wearer for 1 turn.\nYou cannot give the vest to stunned targets.";
    public override Vector2Int Range => Vector2Int.One;
    public override bool RequiresTarget => true;
    public override bool FreeAction => false;
    public override bool UseMoveMarkers => false;
    public override int SortOrder => 1;

    protected override void ActivateEffect(Vector2Int target = null)
    {
        Unit targetUnit = thisUnit.TurnFlowController.GetUnitAtPos(target);
        if (targetUnit != null)
        {
            thisUnit.QueueAnimation(new AnimKnock(), new AnimKnock.Args(0.3f, 30, targetUnit.Pos - thisUnit.Pos, true, false));
            thisUnit.QueueImmediateAction(() =>
            {
                thisUnit.HasVest = false;
                thisUnit.Stunned = true;
                targetUnit.HasVest = true;
                targetUnit.PlaySFX(Unit.SFXType.Begin);
                thisUnit.TurnFlowController.UpdateUI();
            });
            thisUnit.QueueAnimation(new AnimKnock(), new AnimKnock.Args(0.3f, -30, targetUnit.Pos - thisUnit.Pos, true, false));
            thisUnit.QueueAnimation(new AnimDelay(), new AnimDelay.Args(0.5f));
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
        return !target.Stunned;
    }
}

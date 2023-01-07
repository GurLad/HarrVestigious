using Godot;
using System;

public class UAWait : AUnitAction
{
    public override string Name => "Wait";
    public override string Description => "Ends the turn.";
    public override Vector2Int Range => Vector2Int.Zero;
    public override bool RequiresTarget => false;
    public override bool FreeAction => false;
    public override bool UseMoveMarkers => false;

    protected override void ActivateEffect(Vector2Int target = null)
    {
        thisUnit.QueueAnimation(new AnimDelay(), new AnimDelay.Args(0.5f));
    }

    public override void PassiveEffect()
    {
        // Do nothing
    }
}

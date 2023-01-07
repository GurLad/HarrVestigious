using Godot;
using System;
using System.Collections.Generic;

public class UAMove : AUnitAction
{
    public override string Name => "Move";
    public override string Description => "Moves the unit.\nYou can use another action after moving.";
    public override Vector2Int Range => Vector2Int.Zero;
    public override bool RequiresTarget => true;
    public override bool FreeAction => true;
    public override bool UseMoveMarkers => true;

    protected override void ActivateEffect(Vector2Int target = null)
    {
        List<Vector2Int> path = Pathfinder.GetPath(thisUnit.Pos, target);
        foreach (Vector2Int point in path)
        {
            thisUnit.QueueAnimation(new AnimSquash(), new AnimSquash.Args(0.2f, 0.3f, new Vector3(0, -1, 0), true));
            thisUnit.QueueAnimation(new AnimJump(), new AnimJump.Args(0.4f, 0.5f, point - thisUnit.Pos));
            thisUnit.QueueImmediateAction(() => thisUnit.Pos = point);
            thisUnit.QueueAnimation(new AnimSquash(), new AnimSquash.Args(0.3f, 0.3f, new Vector3(0, 1, 0), false));
        }
    }

    public override void PassiveEffect()
    {
        // Do nothing
    }
}

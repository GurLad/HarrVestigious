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
        for (int i = 1; i < path.Count; i++)
        {
            thisUnit.QueueAnimation(new AnimSquash(), new AnimSquash.Args(0.2f, 0.3f, new Vector3(0, 1, 0), false));
            thisUnit.QueueAnimation(new AnimJump(), new AnimJump.Args(0.4f, 0.5f, path[i] - path[i - 1]));
        }
        thisUnit.QueueAnimation(new AnimSquash(), new AnimSquash.Args(0.3f, 0.3f, new Vector3(0, 1, 0), false));
        thisUnit.QueueImmediateAction(() => thisUnit.Pos = target);
    }

    public override void PassiveEffect()
    {
        // Do nothing
    }
}

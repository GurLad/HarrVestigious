using Godot;
using System;

public class UASalSummon : AUnitAction
{
    public static int Mode = 0;

    public override string Name => "Sal";
    public override string Description => "Cheater.";
    public override Vector2Int Range => Vector2Int.Zero;
    public override bool RequiresTarget => false;
    public override bool FreeAction => false;
    public override bool UseMoveMarkers => false;
    public override int SortOrder => 30;

    protected override void ActivateEffect(Vector2Int target = null)
    {
        GD.Print("yes");
        switch (Mode)
        {
            case 0:
                CreateSkelly(new Vector2Int(4, 5));
                CreateSkelly(new Vector2Int(12, 5));
                Mode = 1;
                break;
            case 1:
                CreateOrc(new Vector2Int(5, 2));
                CreateOrc(new Vector2Int(5, 8));
                CreateOrc(new Vector2Int(11, 2));
                CreateOrc(new Vector2Int(11, 8));
                Mode = 2;
                break;
            case 2:
                CreateSkelly(new Vector2Int(8, 1));
                Mode = 3;
                break;
            case 3:
                CreateOrc(new Vector2Int(5, 2));
                CreateOrc(new Vector2Int(5, 8));
                CreateOrc(new Vector2Int(11, 2));
                CreateOrc(new Vector2Int(11, 8));
                Mode = 0;
                break;
            default:
                break;
        }
        thisUnit.QueueAnimation(new AnimDelay(), new AnimDelay.Args(1f));
    }

    public override void PassiveEffect()
    {
        // Do nothing
    }

    public override bool ValidTarget(Unit target)
    {
        return true;
    }

    private void CreateSkelly(Vector2Int pos)
    {
        if (thisUnit.TurnFlowController.GetUnitAtPos(pos) == null)
        {
            Unit unit = LevelGenerator.SalSkeleton.Instance<Unit>();
            CreateUnit(unit, pos);
            LevelGenerator.SalHolder.AddChild(unit);
        }
    }

    private void CreateOrc(Vector2Int pos)
    {
        if (thisUnit.TurnFlowController.GetUnitAtPos(pos) == null)
        {
            Unit unit = LevelGenerator.SalOrc.Instance<Unit>();
            CreateUnit(unit, pos);
            LevelGenerator.SalHolder.AddChild(unit);
        }
    }

    private void CreateUnit(Unit unitObject, Vector2Int pos)
    {
        GD.Print("Creating");
        unitObject.Init();
        unitObject.Pos = pos;
        unitObject.HasVest = false;
        unitObject.FloorMarker = thisUnit.FloorMarker;
        unitObject.TurnFlowController = thisUnit.TurnFlowController;
        unitObject.PlayerUIController = thisUnit.PlayerUIController;
        unitObject.Translate(pos.To3D());
        thisUnit.TurnFlowController.AddUnit(unitObject);
        Pathfinder.PlaceObject(pos);
        unitObject.QueueAnimation(new AnimSummon(), new AnimSummon.Args(1, unitObject.Scale));
        unitObject.Scale = Vector3.Zero;
    }
}

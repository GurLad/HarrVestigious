using Godot;
using System;
using System.Collections.Generic;

public class TurnFlowController : Node
{
    [Export]
    public PackedScene BasePanel;
    [Export]
    public NodePath PanelHolderPath;
    public bool Running;
    private Control panelHolder;
    private List<UnitPanel> allUnits = new List<UnitPanel>();
    private int currentUnit;

    public override void _Ready()
    {
        base._Ready();
        panelHolder = GetNode<Control>(PanelHolderPath);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (Running)
        {
            if (allUnits[currentUnit].Unit.Moved)
            {
                NextUnit();
            }
        }
    }

    public void AddUnit(Unit unit)
    {
        UnitPanel unitPanel = BasePanel.Instance<UnitPanel>();
        unitPanel.Init();
        unitPanel.UpdateUnitData(unit);
        allUnits.Add(unitPanel);
        panelHolder.AddChild(unitPanel);
    }

    public void RemoveUnit(Unit unit)
    {
        UnitPanel panel = allUnits.Find(a => a.Unit == unit);
        if (panel != null)
        {
            if (unit.HasVest)
            {
                Lose();
            }
            else
            {
                Pathfinder.RemoveObject(panel.Unit.Pos);
                allUnits.Remove(panel);
                panel.Unit.QueueFree();
                panel.QueueFree();
            }
        }
        else
        {
            throw new Exception("Killing a non-existent unit!");
        }
    }

    public void RemoveAllUnits()
    {
        while (allUnits.Count > 0)
        {
            UnitPanel panel = allUnits[0];
            Pathfinder.RemoveObject(panel.Unit.Pos);
            allUnits.Remove(panel);
            panel.Unit.QueueFree();
            panel.QueueFree();
        }
    }

    public void MarkUnit(Unit unit)
    {
        UnitPanel panel = allUnits.Find(a => a.Unit == unit);
        if (panel != null)
        {
            panel.Mark();
        }
        else
        {
            throw new Exception("Marking a non-existent unit!");
        }
    }

    public void UnmarkUnit(Unit unit)
    {
        UnitPanel panel = allUnits.Find(a => a.Unit == unit);
        if (panel != null)
        {
            panel.Unmark();
        }
        else
        {
            throw new Exception("Marking a non-existent unit!");
        }
    }

    public Unit GetUnitAtPos(Vector2Int pos)
    {
        return allUnits.Find(a => a.Unit.Pos == pos)?.Unit;
    }

    public void Begin()
    {
        currentUnit = allUnits.FindIndex(a => a.Unit.HasVest);
        ActivateUnit(currentUnit);
        Running = true;
    }

    public void UpdateUI()
    {
        allUnits.ForEach(a => a.UpdateUnitData(a.Unit));
    }

    private void NextUnit()
    {
        int nextTarget = (currentUnit + 1) % allUnits.Count;
        if (!allUnits[nextTarget].Unit.Animating)
        {
            if (currentUnit != nextTarget)
            {
                DeactivateUnit(currentUnit);
                ActivateUnit(nextTarget);
            }
            else
            {
                allUnits[currentUnit].Unit.Moved = false;
                allUnits[currentUnit].Unit.BeginTurn();
            }
        }
    }

    public void Win()
    {
        Running = false;
        EmitSignal("GameOver", true);
    }

    public void Lose()
    {
        Running = false;
        EmitSignal("GameOver", false);
    }

    private void DeactivateUnit(int index)
    {
        allUnits[index].Unit.Moved = false;
        allUnits[index].Deactivate();
    }

    private void ActivateUnit(int index)
    {
        allUnits[currentUnit = index].Activate(() => allUnits[index].Unit.BeginTurn());
    }

    // Signals

    [Signal]
    public delegate void GameOver(bool won);
}

using Godot;
using System;
using System.Collections.Generic;

public class TurnFlowController : Node
{
    public List<Unit> AllUnits = new List<Unit>();
    private int currentUnit;

    public void GenerateUI()
    {

        currentUnit = AllUnits.Count - 1;
    }

    public void NextUnit()
    {

    }
}

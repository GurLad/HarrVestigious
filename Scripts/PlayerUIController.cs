using Godot;
using System;
using System.Collections.Generic;

public class PlayerUIController : Node
{
    [Export]
    public NodePath ButtonContainerPath;
    [Export]
    public NodePath HelpLabelPath;
    [Export]
    public NodePath CancelButtonPath;
    [Export]
    public PackedScene ActionButtonScene;
    public Label HelpLabel;
    private Control buttonContainer;
    private List<ActionButton> buttons = new List<ActionButton>();
    private Unit currentUnit;
    private Button cancelButton;

    public override void _Ready()
    {
        base._Ready();
        buttonContainer = GetNode<Container>(ButtonContainerPath);
        HelpLabel = GetNode<Label>(HelpLabelPath);
        cancelButton = GetNode<Button>(CancelButtonPath);
    }

    public void ShowUI(Unit unit)
    {
        currentUnit = unit;
        foreach (var action in unit.Actions)
        {
            if (action.Exhausted)
            {
                continue;
            }
            ActionButton newButton = ActionButtonScene.Instance<ActionButton>();
            newButton.Init(action, this, unit);
            buttonContainer.AddChild(newButton);
            buttons.Add(newButton);
        }
    }

    public void HideUI(bool showCancelButton)
    {
        HelpLabel.GetParent<Control>().Visible = false;
        while (buttons.Count > 0)
        {
            buttons[0].QueueFree();
            buttons.RemoveAt(0);
        }
        cancelButton.Visible = showCancelButton;
    }

    public void _OnCancel()
    {
        cancelButton.Visible = false;
        currentUnit.FloorMarker.ClearMarkers(FloorMarker.MarkType.Move);
        currentUnit.FloorMarker.ClearMarkers(FloorMarker.MarkType.Attack);
        currentUnit.BeginTurn(false);
    }
}

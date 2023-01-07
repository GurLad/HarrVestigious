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
    public PackedScene ActionButtonScene;
    public Label HelpLabel;
    private Control buttonContainer;
    private List<ActionButton> buttons = new List<ActionButton>();

    public override void _Ready()
    {
        base._Ready();
        buttonContainer = GetNode<Container>(ButtonContainerPath);
        HelpLabel = GetNode<Label>(HelpLabelPath);
    }

    public void ShowUI(Unit unit)
    {
        foreach (var action in unit.Actions)
        {
            ActionButton newButton = ActionButtonScene.Instance<ActionButton>();
            newButton.Init(action, this, unit);
            buttonContainer.AddChild(newButton);
            buttons.Add(newButton);
        }
    }

    public void HideUI()
    {
        HelpLabel.GetParent<Control>().Visible = false;
        while (buttons.Count > 0)
        {
            buttons[0].QueueFree();
            buttons.RemoveAt(0);
        }
    }
}

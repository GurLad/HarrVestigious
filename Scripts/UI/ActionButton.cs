using Godot;
using System;

public class ActionButton : Button
{
    public AUnitAction Action;
    private PlayerUIController uiController;
    private Unit player;

    public void Init(AUnitAction action, PlayerUIController playerUIController, Unit unit)
    {
        player = unit;
        uiController = playerUIController;
        Action = action;
        Text = action.Name;
    }

    public void _OnMouseEntered()
    {
        uiController.HelpLabel.Text = Action.Description;
        uiController.HelpLabel.GetParent<Control>().Visible = true;
    }

    public void _OnMouseLeave()
    {
        uiController.HelpLabel.GetParent<Control>().Visible = false;
    }

    public void _OnPressed()
    {
        base._Pressed();
        uiController.HideUI();
        if (Action.RequiresTarget)
        {

        }
        else
        {
            Action.Activate();
        }
    }
}

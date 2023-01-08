using Godot;
using System;
using System.Collections.Generic;

public class TutorialController : Node
{
    [Export]
    public List<List<string>> Tutorials;
    [Export]
    public NodePath FullPanelPath;
    [Export]
    public NodePath LabelPath;
    private int currentLevel;
    private int currentTutorial;
    private Control fullPanel;
    private Label label;

    public override void _Ready()
    {
        base._Ready();
        fullPanel = GetNode<Control>(FullPanelPath);
        label = GetNode<Label>(LabelPath);
    }

    public void NewLevel(int number)
    {
        currentLevel = number;
        currentTutorial = -1;
    }

    public bool NextTutorial()
    {
        currentTutorial++;
        if (currentTutorial >= Tutorials[currentLevel].Count)
        {
            fullPanel.Visible = false;
            return true;
        }
        else
        {
            fullPanel.Visible = true;
            label.Text = Tutorials[currentLevel][currentTutorial].Replace(@"\n", "\n");
            return false;
        }
    }
}

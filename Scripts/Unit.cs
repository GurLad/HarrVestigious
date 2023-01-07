using Godot;
using System;
using System.Collections.Generic;

public class Unit : Spatial
{
    // Stats
    [Export]
    public int Health;
    [Export]
    public int Soul;
    [Export]
    public int Attack;
    [Export]
    public int Defense;
    [Export]
    public int Movement;
    // Data
    public Vector2Int Pos;
    public List<AUnitAction> Actions = new List<AUnitAction>();
    // For animations
    private AAnimation currentAnimation;
    private Queue<Action> actionQueue = new Queue<Action>();
    private UnitAnchorAnimations anchorAnimations;

    public void Move(Vector2Int target) // Move is technically an action that everything has - TODO: move to an action
    {
        // TODO: play move animation
        actionQueue.Enqueue(() => Pos = target);
    }

    public override void _Ready()
    {
        base._Ready();
        anchorAnimations = GetNode<UnitAnchorAnimations>("Anchor");
        anchorAnimations.AddAnimation(UnitAnchorAnimations.Mode.Breath);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if ((currentAnimation?.Done ?? true) && actionQueue.Count > 0)
        {
            if (currentAnimation != null)
            {
                // Run one more frame to make sure it actually ends on 1
                currentAnimation.AnimateFrame(1);
                currentAnimation.QueueFree();
            }
            actionQueue.Dequeue().Invoke();
        }
    }

    public void QueueAnimation<T, S>(T animation, S animationArgs) where S : AAnimationArgs where T : AAnimation<S>
    {
        actionQueue.Enqueue(() =>
        {
            AddChild(animation);
            currentAnimation = animation.Begin(this, animationArgs);
        });
    }

    public void QueueImmediateAction(Action action)
    {
        actionQueue.Enqueue(action);
    }
}

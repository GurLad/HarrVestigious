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
    public List<AUnitAction> Actions;
    // For animations
    private AAnimation currentAnimation;
    private Queue<Action> actionQueue;

    public void Move(Vector2Int target) // Move is technically an action that everything has - TODO: move to an action
    {
        // TODO: play move animation
        actionQueue.Enqueue(() => Pos = target);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if ((currentAnimation?.Done ?? true) && actionQueue.Count > 0)
        {
            actionQueue.Dequeue().Invoke();
        }
    }

    public void QueueAnimation(AAnimation animation, AAnimationArgs animationArgs)
    {
        actionQueue.Enqueue(() => currentAnimation = animation.Begin(this, animationArgs));
    }

    public void QueueImmediateAction(Action action)
    {
        actionQueue.Enqueue(action);
    }
}

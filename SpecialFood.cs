using Godot;
using System;

public class SpecialFood : Area2D
{
    [Signal] public delegate void Eated();

    private Timer _foodTimer;

    public override void _Ready() {
        _foodTimer = GetNode<Timer>("FoodTimer");
        _foodTimer.WaitTime = 5f;
        _foodTimer.Start();
    }

    public void onBodyEntered(Snake snake) {
        EmitSignal(nameof(Eated), snake);
        QueueFree();
    }

    public void onFoodTimeout() {
        QueueFree();
    }

    public void Destroy() {
        QueueFree();
    }
}

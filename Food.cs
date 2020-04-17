using Godot;
using System;

public class Food : Area2D
{
	[Signal] public delegate void Eated();

	public void onBodyEntered(Snake snake) {
		EmitSignal(nameof(Eated), snake);
		QueueFree();
	}

	public void Destroy() {
        QueueFree();
    }
}

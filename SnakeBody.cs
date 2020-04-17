using Godot;
using System;

public class SnakeBody : Area2D {

    [Signal] delegate void GameOver();

    public void onBodyEntered(Node snake) {
        EmitSignal(nameof(GameOver));
    }
}

using Godot;
using System;

public class GameOver : Control {
    [Signal] public delegate void StartGame();

    public override void _Input(InputEvent @event) {
        if(Visible && @event.IsPressed()) {
            Visible = false;
            EmitSignal(nameof(StartGame));
        }
    }
}

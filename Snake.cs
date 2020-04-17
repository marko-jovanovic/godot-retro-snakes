using Godot;
using System;
using System.Collections.Generic;

public class Snake : Node2D
{
	enum MoveMode { REGULAR, DASH };

	[Signal] public delegate void GameOver();
	[Signal] public delegate void OnSnakeMove(Vector2 previousPosition);
	[Signal] public delegate void UpdateDashTime(float dashTime);

	public HeadedDirections direction;

	public Vector2 previousPosition;

	private Vector2 _velocity;
	private List<Node2D> _snakeBody = new List<Node2D>();

	private float _moveTime = 0;
	private float _moveTimeLimit = 0.4f;
	private float _previousMoveTimeLimit;
	private MoveMode _moveMode = MoveMode.REGULAR;

	private Vector2 _screenSize;

	private float _dashTime = 1f;

	// Set movement step by 16px so the snake move 16 by 16 pixels each move
	// since the size of snake itself is 16px.
	private const int MOVE_STEP = 16;

	public override void _Ready() {
		// Set snake to move up by default
		_velocity = new Vector2(0, -MOVE_STEP);
		_previousMoveTimeLimit = _moveTimeLimit;
		direction = HeadedDirections.UP;
	}

	public override void _PhysicsProcess(float delta) {
		GetInput();
		GetDashInput();
		CalculateDashTime(delta);
		EmitSignal(nameof(UpdateDashTime), _dashTime);
		
		_moveTime += delta;
		if (_moveTime > _moveTimeLimit) {
			previousPosition = this.Position;
			
			this.Position += _velocity;
			_moveTime = 0;

			EmitSignal(nameof(OnSnakeMove), this);
		}
	}

	public void onLevelUp() {
		// Speed up on level up
		if (_moveMode == MoveMode.DASH) {
			if(_previousMoveTimeLimit > 0.1f) {
				_previousMoveTimeLimit -= 0.05f;
			}
		}

		if(_moveMode == MoveMode.REGULAR) {
			if(_moveTimeLimit > 0.1f) {
				_moveTimeLimit -= 0.05f;
			}
		}
	}

	public void onGameOver() {
		EmitSignal(nameof(GameOver));
		QueueFree();
	}

	public void GetInput() {
		if (direction != HeadedDirections.LEFT && Input.IsActionPressed("ui_right")) {
			_velocity.x = MOVE_STEP;
			_velocity.y = 0;
			direction = HeadedDirections.RIGHT;
		}

		if (direction != HeadedDirections.RIGHT && Input.IsActionPressed("ui_left")) {
			_velocity.x = -MOVE_STEP;
			_velocity.y = 0;
			direction = HeadedDirections.LEFT;
		}

		if (direction != HeadedDirections.DOWN && Input.IsActionPressed("ui_up")) {
			_velocity.y = -MOVE_STEP;
			_velocity.x = 0;
			direction = HeadedDirections.UP;
		}

		if (direction != HeadedDirections.UP && Input.IsActionPressed("ui_down")) {
			_velocity.y = MOVE_STEP;
			_velocity.x = 0;
			direction = HeadedDirections.DOWN;
		}
	}

	public void GetDashInput() {
		if(Input.IsActionJustPressed("ui_accept")) {
			_previousMoveTimeLimit = this._moveTimeLimit;
			_moveTimeLimit = 0.05f;
			_moveMode = MoveMode.DASH;
		}

		if(Input.IsActionJustReleased("ui_accept")) {
			_moveTimeLimit = this._previousMoveTimeLimit;
			_moveMode = MoveMode.REGULAR;
		}
	}

	public void CalculateDashTime(float deltaTime) {
		if (_moveMode == MoveMode.REGULAR) {
			_dashTime = (_dashTime < 1f) ? (_dashTime + deltaTime / 4) : _dashTime;
		}
		
		if (_moveMode == MoveMode.DASH) {
			if (_dashTime <= 0) {
				_moveTimeLimit = this._previousMoveTimeLimit;
				_moveMode = MoveMode.REGULAR;
			} else {
				_dashTime -= deltaTime;
			}
		}
	}
}


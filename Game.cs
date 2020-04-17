using Godot;
using System;
using System.Collections.Generic;

public class Game : Node {
	
	[Export] public PackedScene snakeBodyPart;

	[Export] public PackedScene snakeFood;

	[Export] public PackedScene specialSnakeFood;

	[Export] public PackedScene snake;

	private List<Node2D> _snakeBodyList = new List<Node2D>();

	private GUI _gui;
	private Control _gameOverScreen;

	private Timer _specialFoodSpawnTimer;

	private Vector2 _screenSize;

	private Random _randomGenerator;

	public override void _Ready() {
		_gui = GetNode<GUI>("GUI");
		_randomGenerator = new Random();
		
		_gameOverScreen = GetNode<Control>("GameOverScreen");
		_gameOverScreen.Connect("StartGame", this, nameof(onStartGame));

		initializeGame();
	}

	public void onFoodEated(Snake snake) {
		this.growSnakeBody(snake);
		_gui.updateScore(FoodType.REGULAR);
	
		randomlySpawnFood();
	}

	public void onSpecialFoodEated(Snake snake) {
		this.growSnakeBody(snake);
		_gui.updateScore(FoodType.SPECIAL);
	}

	public void onSnakeMove(Snake snake) {
		// If snake reaches the end of map, move it to the other side of the map
		if(snake.Position.x > _screenSize.x) {
			snake.Position = new Vector2(8, snake.Position.y);
		}

		if(snake.Position.x < 0) {
			snake.Position = new Vector2(_screenSize.x - 8, snake.Position.y);
		}

		if(snake.Position.y > _screenSize.y) {
			snake.Position = new Vector2(snake.Position.x, 8);
		}

		if(snake.Position.y < 0) {
			snake.Position = new Vector2(snake.Position.x, _screenSize.y - 8);
		}

		// Move rest of the snake body to the position snake's had was previously
		Vector2 previousPosition = snake.previousPosition;
		foreach (Node2D snakeBodyPart in _snakeBodyList) {
			var tempPosition = snakeBodyPart.Position;

			snakeBodyPart.Position = previousPosition;
			previousPosition = tempPosition;
		}
	}

	public void onSpecialFoodDestory() {
		startSpecialFoodSpawnCounter();
	}

	public void onSpecialFoodSpawnTimeout() {
		var newSpecialSnakeFood = specialSnakeFood.Instance() as SpecialFood;
		newSpecialSnakeFood.Position = generateRandomPosition();
		newSpecialSnakeFood.AddToGroup("SpecialFood");
		newSpecialSnakeFood.Connect("tree_exited", this, nameof(onSpecialFoodDestory));
		newSpecialSnakeFood.Connect("Eated", this, nameof(onSpecialFoodEated));

		CallDeferred("add_child", newSpecialSnakeFood);
	}

	private void randomlySpawnFood() {
		var newSnakeFood = snakeFood.Instance() as Node2D;
		newSnakeFood.Position = generateRandomPosition();
		newSnakeFood.AddToGroup("Food");
		newSnakeFood.Connect("Eated", this, nameof(onFoodEated));

		CallDeferred("add_child", newSnakeFood, true);
	}

	public void onGameOver() {
		// Remove all food on screen
		GetTree().CallGroup("Food", "Destroy");
		GetTree().CallGroup("SpecialFood", "Destroy");

		// Stop special food spawn
		_specialFoodSpawnTimer.Stop();

		// Show game over screen
		_gameOverScreen.Visible = true;

		// Write high score
		_gui.updateHighScore();

		// Erase whole snake body
		foreach(Node2D snakeBodyPart in _snakeBodyList) {
			snakeBodyPart.QueueFree();
		}
		_snakeBodyList = new List<Node2D>();
	}

	public void onStartGame() {
		initializeGame();
		_gui.resetUI();
	}

	private void initializeGame() {
		// We don't want our snake to be able to go through GUI, so we subtract 
		// the width of GUI to limit the snake movement and food spawn.
		var viewportSize = GetViewport().Size;
		_screenSize = new Vector2(viewportSize.x - _gui.RectSize.x, viewportSize.y);

		// Prepare special food spawn timer
		_specialFoodSpawnTimer = GetNode<Timer>("SpecialFoodSpawnTimer");
		_specialFoodSpawnTimer.Connect("timeout", this, nameof(onSpecialFoodSpawnTimeout));
		startSpecialFoodSpawnCounter();

		// Initialize game
		spawnSnake();
		randomlySpawnFood();
	}

	private void growSnakeBody(Snake snake) {
		SnakeBody grownBodyPart = snakeBodyPart.Instance() as SnakeBody;
		// Since the whole snake body follows snake's head, it doesn't matter on which
		// position the body part will be spawned. We set this -100, -100 so the instantiation
		// would not be visible to the player. Side effect of this approach is that the snake
		// looks like it grows after one move, not immediately on collision.
		grownBodyPart.Position = new Vector2(-100, -100);
		
		// There is no way for snake's head to bite first 2 body elements
		if(_snakeBodyList.Capacity >= 3) {
			grownBodyPart.Connect("GameOver", snake, nameof(snake.onGameOver));
		}

		_snakeBodyList.Add(grownBodyPart);
		CallDeferred("add_child", grownBodyPart);
	}

	private void spawnSnake() {
		float screenCenterX = Mathf.Round(_screenSize.x / 2);
		float screenCenterY = Mathf.Round(_screenSize.y / 2);

		var snakePosX = screenCenterX % 16 == 0 ? screenCenterX : (screenCenterX - (screenCenterX % 16));
		var snakePosY = screenCenterY % 16 == 0 ? screenCenterY : (screenCenterY - (screenCenterY % 16));

		snakePosX -= 8;
		snakePosY -= 8;

		// Spawn the snake at the center of the map
		Snake snake = this.snake.Instance() as Snake;
		snake.Position = new Vector2(snakePosX, snakePosY);
		snake.Connect("OnSnakeMove", this, nameof(onSnakeMove));
		snake.Connect("UpdateDashTime", _gui, nameof(_gui.onUpdateDashTime));
		snake.Connect("GameOver", this, nameof(onGameOver));
		_gui.Connect("LevelUp", snake, nameof(snake.onLevelUp));
		

		CallDeferred("add_child", snake);
	}

	private Vector2 generateRandomPosition() {
		int randomXPos = _randomGenerator.Next(8, (int) _screenSize.x);
		int randomYPos = _randomGenerator.Next(8, (int) _screenSize.y);

		// We want coordinates to snap (be divisible by 16)
		randomXPos = (randomXPos % 16 == 0) ? randomXPos : Math.Abs((randomXPos - (randomXPos % 16)));
		randomYPos = (randomYPos % 16 == 0) ? randomYPos : Math.Abs((randomYPos - (randomYPos % 16)));

		// Since 16 is the whole tile width it cannot be used as coordinate since the position of
		// object is determined by pivot, and pivot is placed at the center of the object.
		// So the center of 16x16 tile is 8x8.
		
		// Make sure that position is not 0
		randomXPos = randomXPos == 0 ? 8 : randomXPos;
		randomYPos = randomYPos == 0 ? 8 : randomYPos;

		// Make sure to subtract by 8 only of coords are not 8
		randomXPos = randomXPos == 8 ? randomXPos : (randomXPos - 8);
		randomYPos = randomYPos == 8 ? randomYPos : (randomYPos - 8);

		return new Vector2(randomXPos, randomYPos);
	}

	private void startSpecialFoodSpawnCounter() {
		_specialFoodSpawnTimer.WaitTime = _randomGenerator.Next(10, 20);
		_specialFoodSpawnTimer.Start();
	}
}

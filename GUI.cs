using Godot;
using System.Collections.Generic;
using System;

public class GUI : PanelContainer
{
	[Signal] delegate void LevelUp();

	[Export] int pointsPerFood = 10;
	[Export] int pointPerSpecialFood = 50;

	private Label _score;
	private Label _hiScore;
	private Label _level;
	private TextureProgress _dashProgress;

	private List<TextureRect> _nextLevelIndicators = new List<TextureRect>();
	private int _currentFoodCount = 0;

	// Textures
	private Texture backgroundLight;
	private Texture backgroundDark;

	public override void _Ready() {
		// Preload textures
		backgroundLight = GD.Load("res://assets/light-box.png") as Texture;
		backgroundDark = GD.Load("res://assets/black-box.png") as Texture;

		// Get UI references
		_score = FindNode("ScoreValue", true, true) as Label;
		_hiScore = FindNode("HiScoreValue", true, true) as Label;
		_level = FindNode("LevelValue", true, true) as Label;
		_dashProgress = FindNode("DashProgress", true, true) as TextureProgress;

		for(int i = 1; i <= 12; i++) {
			_nextLevelIndicators.Add(FindNode("LevelProgressItem" + i) as TextureRect);            
		}

		// Set initial values
		_score.Text = 0.ToString();
		_hiScore.Text = 0.ToString();
		_level.Text = 1.ToString();
	}

	public void onUpdateDashTime(float dashTime) {
		double dashTimePercent = dashTime * 100;
		_dashProgress.Value = dashTimePercent;
	}

	public void updateScore(FoodType foodType) {
		switch(foodType) {
            case FoodType.REGULAR:
                _score.Text = (Int32.Parse(_score.Text) + pointsPerFood).ToString();
            break;
            case FoodType.SPECIAL:
                _score.Text = (Int32.Parse(_score.Text) + pointPerSpecialFood).ToString();
            break;
        }
        
        updateLevelIndicator();
	}

	public void updateHighScore() {
		_hiScore.Text = _score.Text;
	}

	public void resetUI() {
		_score.Text = 0.ToString();
		_level.Text = 1.ToString();

		foreach(TextureRect nextLevelIndicator in _nextLevelIndicators) {
			nextLevelIndicator.Texture = backgroundLight;
		}
		_dashProgress.Value = 100f;
	}

    private void updateLevelIndicator() {
        if (_currentFoodCount == 12) {
            foreach(TextureRect nextLevelIndicator in _nextLevelIndicators) {
                nextLevelIndicator.Texture = backgroundLight;
            }
            _currentFoodCount = 0;
			_level.Text = (Int32.Parse(_level.Text) + 1).ToString();
			EmitSignal(nameof(LevelUp));
        }

		_nextLevelIndicators[_currentFoodCount++].Texture = backgroundDark;
    }
}

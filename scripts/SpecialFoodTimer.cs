using System;

public class SpecialFoodTimer {
    // NOTE: all time is expressed in seconds
    enum SpawnTimeState { UPDATE_SPAWN_DURATION, UPDATE_FOOD_DURATION, GET_SPAWN_DURATION };

    private const float MAX_FOOD_DURATION = 5f;

    private int _minSpawnTime;
    private int _maxSpawnTime;
    private float _currentSpawnTime = 0f;
    private float _spawnTime = 0;

    private SpawnTimeState _timerState = SpawnTimeState.GET_SPAWN_DURATION;

    private Random _randomGenerator = new Random();

    public SpecialFoodTimer(int minSpawnTime, int maxSpawnTime) {
        _minSpawnTime = minSpawnTime;
        _maxSpawnTime = maxSpawnTime;
    }

    public bool updateSpawnTimer(float time) {
        
        switch(_timerState) {
            case SpawnTimeState.GET_SPAWN_DURATION:
                _spawnTime = _randomGenerator.Next(_minSpawnTime, _maxSpawnTime);
                _timerState = SpawnTimeState.UPDATE_SPAWN_DURATION;
            break;

            case SpawnTimeState.UPDATE_SPAWN_DURATION:
                _currentSpawnTime += time;

                if(_currentSpawnTime > _spawnTime) {
                    _currentSpawnTime = 0f;
                    _timerState = SpawnTimeState.UPDATE_FOOD_DURATION;

                    return true;
                }
            break;
        }

        return false;
    }
}
using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameTimer _gameTimer;
    [SerializeField] private ExitZone _exitZone;
    [SerializeField] private Player _player;

    public event Action<bool> GameOverTriggered;

    private void Start()
    {
        Time.timeScale = 1;
        _gameTimer.TimeLeftChanged += OnTimeLeftChanged;
        _exitZone.ExitZoneReached += Win;
        _player.HealthChanged += OnPlayerHealthChanged;
    }

    private void OnDestroy()
    {
        _gameTimer.TimeLeftChanged -= OnTimeLeftChanged;
        _exitZone.ExitZoneReached -= Win;
        _player.HealthChanged -= OnPlayerHealthChanged;
    }
    
    private void OnPlayerHealthChanged()
    {
        if (_player.Health <= 0)
            GameOver(false);
    }

    private void OnTimeLeftChanged(int timeLeft)
    {
        if (timeLeft == 0)
            GameOver(false);
    }
    
    private void Win() => GameOver(true);

    private void GameOver(bool win)
    {
        Time.timeScale = 0;
        FindObjectOfType<Player>().enabled = false;
        GameOverTriggered?.Invoke(win);
    }
}

using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameTimer _gameTimer;
    [SerializeField] private ExitZone _exitZone;
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _pausePanel; // Add this in Unity Inspector

    public event Action<bool> GameOverTriggered;

    private bool _isPaused = false;
    public bool IsPaused => _isPaused; // Public property for other scripts to check
    private const KeyCode PauseKey = KeyCode.Escape;

    private void Start()
    {
        Time.timeScale = 1;
        _gameTimer.TimeLeftChanged += OnTimeLeftChanged;
        _exitZone.ExitZoneReached += Win;
        _player.HealthChanged += OnPlayerHealthChanged;

        // Initialize pause panel as hidden
        if (_pausePanel != null)
            _pausePanel.SetActive(false);
    }

    private void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(PauseKey))
        {
            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }
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

    public void PauseGame()
    {
        if (_isPaused) return;

        _isPaused = true;
        Time.timeScale = 0f; // Freezes all time-based operations

        if (_pausePanel != null)
            _pausePanel.SetActive(true);

        Debug.Log("Game Paused - Press ESC to resume");
    }

    public void ResumeGame()
    {
        if (!_isPaused) return;

        _isPaused = false;
        Time.timeScale = 1f; // Resumes normal time

        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        Debug.Log("Game Resumed");
    }

    // Public method for UI buttons (optional)
    public void TogglePause()
    {
        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }
}
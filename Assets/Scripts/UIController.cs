using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _health;
    [SerializeField] private TextMeshProUGUI _timeLeft;
    [SerializeField] private RectTransform _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _gameOverText;
    [SerializeField] private Button _titleScreenButton;
    [SerializeField] private GameController _gameController;
    [SerializeField] private Player _player;
    [SerializeField] private GameTimer _timer;

    private void Start()
    {
        _gameOverPanel.gameObject.SetActive(false);
        
        _player.HealthChanged += SetHealthText;
        _timer.TimeLeftChanged += SetTimerText;
        _gameController.GameOverTriggered += OnGameOverTriggered;
        _titleScreenButton.onClick.AddListener(OnTitleScreenButtonClicked);
        
        SetTimerText(_timer.TimeLeft);
    }

    private void OnDestroy()
    {
        _player.HealthChanged -= SetHealthText;
        _timer.TimeLeftChanged -= SetTimerText;
        _gameController.GameOverTriggered -= OnGameOverTriggered;
        _titleScreenButton.onClick.RemoveAllListeners();
    }
    
    private void OnTitleScreenButtonClicked() => SceneManager.LoadScene(0);
    private void SetHealthText() => _health.text = _player.Health.ToString();
    private void SetTimerText(int timeLeft) => _timeLeft.text = $"{timeLeft / 60:D2}:{timeLeft % 60:D2}";
    private void OnGameOverTriggered(bool win)
    {
        _gameOverPanel.gameObject.SetActive(true);
        _gameOverText.text = win ? "YOU WIN!" : "GAME OVER";
    }
}

// using TMPro;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

// public class UIController : MonoBehaviour
// {
//     [SerializeField] private TextMeshProUGUI _health;
//     [SerializeField] private TextMeshProUGUI _timeLeft;
//     [SerializeField] private RectTransform _gameOverPanel;
//     [SerializeField] private TextMeshProUGUI _gameOverText;
//     [SerializeField] private Button _titleScreenButton;
//     [SerializeField] private GameController _gameController;
//     [SerializeField] private Player _player;
//     [SerializeField] private GameTimer _timer;

//     private void Start()
//     {
//         _gameOverPanel.gameObject.SetActive(false);
        
//         _player.HealthChanged += SetHealthText;
//         _timer.TimeLeftChanged += SetTimerText;
//         _gameController.GameOverTriggered += OnGameOverTriggered;
//         _titleScreenButton.onClick.AddListener(OnTitleScreenButtonClicked);
        
//         SetTimerText(_timer.TimeLeft);
//     }

//     private void OnDestroy()
//     {
//         _player.HealthChanged -= SetHealthText;
//         _timer.TimeLeftChanged -= SetTimerText;
//         _gameController.GameOverTriggered -= OnGameOverTriggered;
//         _titleScreenButton.onClick.RemoveAllListeners();
//     }
    
//     private void OnTitleScreenButtonClicked() => SceneManager.LoadScene(0);
//     private void SetHealthText() => _health.text = _player.Health.ToString();
//     private void SetTimerText(int timeLeft) => _timeLeft.text = $"{timeLeft / 60:D2}:{timeLeft % 60:D2}";
//     private void OnGameOverTriggered(bool win)
//     {
//         _gameOverPanel.gameObject.SetActive(true);
//         _gameOverText.text = win ? "YOU WIN!" : "GAME OVER";
//     }
// }
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // IMPORTANT: Make sure you have this line for the Slider

public class UIController : MonoBehaviour
{
    // MODIFIED: Changed the _health variable from TextMeshProUGUI to Slider
    [SerializeField] private Slider _healthSlider; 
    [SerializeField] private TextMeshProUGUI _timeLeft;
    [SerializeField] private RectTransform _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _gameOverText;
    [SerializeField] private Button _titleScreenButton;
    [SerializeField] private GameController _gameController;
    [SerializeField] private Player _player; // I'm assuming your Player class inherits from LivingEntity
    [SerializeField] private GameTimer _timer;

    private void Start()
    {
        _gameOverPanel.gameObject.SetActive(false);
        
        // MODIFIED: Subscribe the new UpdateHealthBar method to the event
        _player.HealthChanged += UpdateHealthBar; 
        _timer.TimeLeftChanged += SetTimerText;
        _gameController.GameOverTriggered += OnGameOverTriggered;
        _titleScreenButton.onClick.AddListener(OnTitleScreenButtonClicked);
        
        SetTimerText(_timer.TimeLeft);

        // ADDED: Set the health bar to the correct starting value when the game begins.
        UpdateHealthBar();
    }

    private void OnDestroy()
    {
        // MODIFIED: Unsubscribe the new method
        _player.HealthChanged -= UpdateHealthBar;
        _timer.TimeLeftChanged -= SetTimerText;
        _gameController.GameOverTriggered -= OnGameOverTriggered;
        _titleScreenButton.onClick.RemoveAllListeners();
    }
    
    private void OnTitleScreenButtonClicked() => SceneManager.LoadScene(0);

    // DELETED: The old SetHealthText method is no longer needed.
    // private void SetHealthText() => _health.text = _player.Health.ToString();

    // NEW METHOD: This calculates and sets the slider's value.
    private void UpdateHealthBar()
    {
        // Your LivingEntity script already has Health and MaxHealth, which is perfect!
        // We calculate the health percentage and set the slider's value.
        // We cast to float to prevent integer division (which would result in 0 or 1).
        _healthSlider.value = (float)_player.Health / _player.MaxHealth;
    }
    
    private void SetTimerText(int timeLeft) => _timeLeft.text = $"{timeLeft / 60:D2}:{timeLeft % 60:D2}";
    
    private void OnGameOverTriggered(bool win)
    {
        _gameOverPanel.gameObject.SetActive(true);
        _gameOverText.text = win ? "YOU WIN!" : "GAME OVER";
    }
}
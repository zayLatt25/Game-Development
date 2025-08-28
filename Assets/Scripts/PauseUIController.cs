using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private TextMeshProUGUI _pauseTitle;
    [SerializeField] private Button _resumeButton;

    [Header("Game References")]
    [SerializeField] private GameController _gameController;

    private void Start()
    {
        // Make sure pause panel starts hidden
        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        // Connect resume button
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(OnResumeButtonClicked);
    }

    private void OnDestroy()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        // Update the pause panel visibility when game is paused
        if (_gameController != null && _pausePanel != null)
        {
            bool isPaused = _gameController.IsPaused;
            _pausePanel.SetActive(isPaused);
        }
    }

    private void OnResumeButtonClicked()
    {
        if (_gameController != null)
        {
            _gameController.ResumeGame();
        }
    }
}
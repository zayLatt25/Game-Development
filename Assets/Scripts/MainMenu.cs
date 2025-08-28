using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _quitButton;

    private void Start()
    {
        _newGameButton.onClick.AddListener(OnNewGameClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnDestroy()
    {
        _newGameButton.onClick.RemoveAllListeners();
        _quitButton.onClick.RemoveAllListeners();
    }

    private void OnNewGameClicked() => SceneManager.LoadScene(1);
    private static void OnQuitClicked() => Application.Quit();
}

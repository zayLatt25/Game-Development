using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string message;
        public KeyCode requiredKey;
        public TutorialType type;
        public float timeoutDuration = 10f; // Auto-advance after this time if no input
    }

    public enum TutorialType
    {
        Movement,
        Aiming,
        Shooting,
        Pickup,
        Complete
    }

    [Header("Tutorial Settings")]
    [SerializeField] private bool _skipTutorial = false; // For testing
    [SerializeField] private TutorialStep[] _tutorialSteps;

    [Header("UI References")]
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private TextMeshProUGUI _tutorialText;
    [SerializeField] private GameObject _skipButton;
    [SerializeField] private Button _skipButtonComponent;

    [Header("Game References")]
    [SerializeField] private Player _player;
    [SerializeField] private GameTimer _gameTimer;

    [Header("Tutorial Movement Restrictions")]
    [SerializeField] private float _maxDistanceFromStart = 3f; // Maximum distance player can move from start

    private int _currentStepIndex = 0;
    private bool _tutorialActive = false;
    private bool _waitingForInput = false;
    private Coroutine _timeoutCoroutine;
    private Vector3 _playerStartPosition;
    private Vector3 _playerTutorialBoundaryCenter;

    // Events
    public System.Action OnTutorialComplete;

    private void Start()
    {
        InitializeTutorialSteps();
        SetupSkipButton();

        // Find references if not set in inspector
        if (_player == null)
            _player = FindObjectOfType<Player>();
        if (_gameTimer == null)
            _gameTimer = FindObjectOfType<GameTimer>();

        if (_skipTutorial)
        {
            CompleteTutorial();
            return;
        }

        StartTutorial();
    }

    private void SetupSkipButton()
    {
        // Position skip button at top-middle of the screen
        if (_skipButton != null)
        {
            RectTransform skipRect = _skipButton.GetComponent<RectTransform>();
            if (skipRect != null)
            {
                // Set anchor to top-center
                skipRect.anchorMin = new Vector2(0.5f, 1f);
                skipRect.anchorMax = new Vector2(0.5f, 1f);

                // Position it slightly below the top edge
                skipRect.anchoredPosition = new Vector2(0, -50f); // 50 pixels from top

                // Set a reasonable size for the button
                skipRect.sizeDelta = new Vector2(150f, 40f);
            }
        }

        // Connect skip button click event
        if (_skipButtonComponent == null && _skipButton != null)
        {
            _skipButtonComponent = _skipButton.GetComponent<Button>();
        }

        if (_skipButtonComponent != null)
        {
            _skipButtonComponent.onClick.RemoveAllListeners();
            _skipButtonComponent.onClick.AddListener(SkipTutorial);
        }
    }

    private void InitializeTutorialSteps()
    {
        _tutorialSteps = new TutorialStep[]
        {
            new TutorialStep
            {
                message = "Welcome to Last Man Standing!\n\nUse WASD keys to move around\nTry moving now...",
                requiredKey = KeyCode.W, // Any movement key will work
                type = TutorialType.Movement,
                timeoutDuration = 8f
            },
            new TutorialStep
            {
                message = "Great! Move your mouse to aim\nNotice your weapon follows the cursor",
                requiredKey = KeyCode.None, // Mouse movement (no key required)
                type = TutorialType.Aiming,
                timeoutDuration = 5f
            },
            new TutorialStep
            {
                message = "Click LEFT MOUSE BUTTON to shoot\nTry firing your weapon now!",
                requiredKey = KeyCode.Mouse0,
                type = TutorialType.Shooting,
                timeoutDuration = 8f
            },
            new TutorialStep
            {
                message = "Press C to pick up items\nLook for items on the ground and press C when nearby",
                requiredKey = KeyCode.C,
                type = TutorialType.Pickup,
                timeoutDuration = 10f
            },
            new TutorialStep
            {
                message = "Tutorial Complete!\nSurvive for 10 minutes and reach the green exit zone!\nPress ESC anytime to pause\n\nGood luck!",
                requiredKey = KeyCode.None,
                type = TutorialType.Complete,
                timeoutDuration = 3f
            }
        };
    }

    private void Update()
    {
        if (!_tutorialActive) return;

        // Restrict player movement during tutorial
        RestrictPlayerMovement();

        if (!_waitingForInput) return;

        // Check for the required input for current step
        TutorialStep currentStep = _tutorialSteps[_currentStepIndex];
        bool inputDetected = false;

        switch (currentStep.type)
        {
            case TutorialType.Movement:
                // Check for any movement key
                inputDetected = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                               Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) ||
                               Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                               Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);
                break;

            case TutorialType.Aiming:
                // Check if mouse moved significantly
                inputDetected = Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0;
                break;

            case TutorialType.Shooting:
                // Check for shooting input
                inputDetected = Input.GetMouseButtonDown(0);
                break;

            case TutorialType.Pickup:
                inputDetected = Input.GetKeyDown(KeyCode.C);
                break;

            default:
                inputDetected = currentStep.requiredKey != KeyCode.None && Input.GetKeyDown(currentStep.requiredKey);
                break;
        }

        if (inputDetected)
        {
            NextStep();
        }
    }

    private void RestrictPlayerMovement()
    {
        if (_player == null) return;

        // Keep player within bounds of tutorial area
        Vector3 playerPos = _player.transform.position;
        float distanceFromCenter = Vector3.Distance(playerPos, _playerTutorialBoundaryCenter);

        if (distanceFromCenter > _maxDistanceFromStart)
        {
            // Pull player back toward center
            Vector3 directionToCenter = (_playerTutorialBoundaryCenter - playerPos).normalized;
            Vector3 maxPosition = _playerTutorialBoundaryCenter + (-directionToCenter * _maxDistanceFromStart);
            _player.transform.position = maxPosition;
        }
    }

    public void StartTutorial()
    {
        _tutorialActive = true;
        _currentStepIndex = 0;

        // Store player start position for movement restriction
        if (_player != null)
        {
            _playerStartPosition = _player.transform.position;
            _playerTutorialBoundaryCenter = _playerStartPosition;
        }

        // Pause the game timer during tutorial
        if (_gameTimer != null)
            _gameTimer.enabled = false;

        // Show tutorial UI
        if (_tutorialPanel != null)
            _tutorialPanel.SetActive(true);

        // Show skip button
        if (_skipButton != null)
            _skipButton.SetActive(true);

        // Player remains enabled but with restrictions
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        if (_currentStepIndex >= _tutorialSteps.Length)
        {
            CompleteTutorial();
            return;
        }

        TutorialStep currentStep = _tutorialSteps[_currentStepIndex];

        // Update UI text
        if (_tutorialText != null)
            _tutorialText.text = currentStep.message;

        _waitingForInput = true;

        // Start timeout coroutine
        if (_timeoutCoroutine != null)
            StopCoroutine(_timeoutCoroutine);

        _timeoutCoroutine = StartCoroutine(TimeoutStep(currentStep.timeoutDuration));

        Debug.Log($"Tutorial Step {_currentStepIndex + 1}: {currentStep.type}");
    }

    private IEnumerator TimeoutStep(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (_waitingForInput) // Still waiting for input
        {
            NextStep(); // Auto-advance
        }
    }

    private void NextStep()
    {
        _waitingForInput = false;

        if (_timeoutCoroutine != null)
        {
            StopCoroutine(_timeoutCoroutine);
            _timeoutCoroutine = null;
        }

        _currentStepIndex++;

        if (_currentStepIndex >= _tutorialSteps.Length)
        {
            CompleteTutorial();
        }
        else
        {
            // Brief pause between steps
            StartCoroutine(ShowNextStepAfterDelay(1f));
        }
    }

    private IEnumerator ShowNextStepAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowCurrentStep();
    }

    public void CompleteTutorial()
    {
        _tutorialActive = false;
        _waitingForInput = false;

        // Stop any running coroutines
        if (_timeoutCoroutine != null)
        {
            StopCoroutine(_timeoutCoroutine);
            _timeoutCoroutine = null;
        }

        // Hide tutorial UI
        if (_tutorialPanel != null)
            _tutorialPanel.SetActive(false);

        // Hide skip button
        if (_skipButton != null)
            _skipButton.SetActive(false);

        // Resume game timer
        if (_gameTimer != null)
            _gameTimer.enabled = true;

        // Invoke completion event
        OnTutorialComplete?.Invoke();

        Debug.Log("Tutorial completed!");
    }

    public void SkipTutorial()
    {
        if (_tutorialActive)
        {
            Debug.Log("Tutorial skipped by user");
            CompleteTutorial();
        }
    }

    // Public method to check if tutorial is running (for other systems)
    public bool IsTutorialActive()
    {
        return _tutorialActive;
    }

    // Get the movement speed multiplier during tutorial
    public float GetTutorialMovementSpeedMultiplier()
    {
        if (_tutorialActive)
            return 0.5f; // Half speed during tutorial
        return 1f; // Normal speed
    }

    private void OnDestroy()
    {
        // Clean up button listener
        if (_skipButtonComponent != null)
        {
            _skipButtonComponent.onClick.RemoveListener(SkipTutorial);
        }
    }

    // Draw tutorial boundary in editor for debugging
    private void OnDrawGizmosSelected()
    {
        if (_tutorialActive && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_playerTutorialBoundaryCenter, _maxDistanceFromStart);
        }
    }
}
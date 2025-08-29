using System;
using UnityEngine;
using UnityEngine.UI;

public class ExitZone : MonoBehaviour
{
    public event Action ExitZoneReached;

    // Testing toggle - set to false when game is finished
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugOverlay = false;

    private Player _player;
    private SpriteRenderer _spriteRenderer;
    private int _selectedCornerIndex;
    private Text _debugText;

    // Corner coordinates for the exit
    private readonly Vector3[] cornerPositions = new Vector3[]
    {
        new Vector3(75f, 75f, 0),    // Top-Right
        new Vector3(-75f, 45f, 0),   // Top-Left
        new Vector3(-75f, -75f, 0),  // Bottom-Left
        new Vector3(50f, -65f, 0)    // Bottom-Right
    };

    // Corner names for testing display
    private readonly string[] cornerNames = new string[]
    {
        "TOP-RIGHT",
        "TOP-LEFT",
        "BOTTOM-LEFT",
        "BOTTOM-RIGHT"
    };

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Randomly select one of the four corners
        _selectedCornerIndex = UnityEngine.Random.Range(0, 4);
        transform.position = cornerPositions[_selectedCornerIndex];

        Debug.Log($"Exit spawned at {cornerNames[_selectedCornerIndex]}: {transform.position}");
    }

    private void Start()
    {
        // Find and subscribe to player events
        _player = FindObjectOfType<Player>();
        if (_player != null)
        {
            _player.IsInfectedChanged += OnPlayerIsInfectedChanged;
        }

        // Create the EXIT text above the zone
        CreateExitText();

        // Create debug overlay if enabled (for testing)
        if (showDebugOverlay)
        {
            CreateDebugOverlay();
        }
    }

    private void CreateExitText()
    {
        // Create the EXIT text that appears above the exit zone
        GameObject textObj = new GameObject("ExitText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.up * 2f;

        TextMesh text = textObj.AddComponent<TextMesh>();
        text.text = "EXIT";
        text.fontSize = 25;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.green;
        text.characterSize = 0.25f;
    }

    private void CreateDebugOverlay()
    {
        // Find or create a canvas for UI elements
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create background panel for debug text
        GameObject debugPanel = new GameObject("ExitDebugPanel");
        debugPanel.transform.SetParent(canvas.transform);

        Image bgImage = debugPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black background

        // Position panel at top-left corner of screen
        RectTransform panelRect = debugPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);
        panelRect.sizeDelta = new Vector2(200, 60);

        // Create debug text
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(debugPanel.transform);

        _debugText = textObj.AddComponent<Text>();
        _debugText.text = $"EXIT: {cornerNames[_selectedCornerIndex]}\n({transform.position.x:F0}, {transform.position.y:F0})";
        _debugText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _debugText.fontSize = 16;
        _debugText.color = Color.yellow;
        _debugText.alignment = TextAnchor.MiddleCenter;

        // Make text fill the panel
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private void OnPlayerIsInfectedChanged(bool isInfected)
    {
        // Change exit color based on infection status
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = isInfected ? Color.red : Color.green;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if uninfected player reached the exit
        if (!other.isTrigger && other.TryGetComponent<Player>(out var player) && !player.IsInfected)
        {
            ExitZoneReached?.Invoke();
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (_player != null)
        {
            _player.IsInfectedChanged -= OnPlayerIsInfectedChanged;
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthVisualEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player _player;
    [SerializeField] private Image _screenBorderImage; // Assign in Inspector

    [Header("Effect Settings")]
    [SerializeField] private float _lowHealthThreshold = 30f;
    [SerializeField] private float _criticalHealthThreshold = 15f;
    [SerializeField] private Color _damageColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private Color _infectionColor = new Color(0.5f, 0f, 0.5f, 0.3f);

    [Header("Heartbeat Settings")]
    [SerializeField] private AnimationCurve _heartbeatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float _normalHeartbeatSpeed = 1.2f;
    [SerializeField] private float _fastHeartbeatSpeed = 2.5f;
    [SerializeField] private float _criticalHeartbeatSpeed = 4f;

    private Coroutine _heartbeatCoroutine;
    private Coroutine _damageFlashCoroutine;
    private float _currentAlpha = 0f;
    private bool _isInfected = false;
    private int _lastHealth = 100;
    private const int MAX_HEALTH = 100; // Since we can't access MaxHealth directly

    private void Start()
    {
        // Create screen border if not assigned
        if (_screenBorderImage == null)
        {
            CreateScreenBorder();
        }

        // Subscribe to player events
        if (_player != null)
        {
            _player.HealthChanged += OnHealthChanged;
            _player.IsInfectedChanged += OnInfectionChanged;
            _lastHealth = _player.Health;
        }

        // Start with transparent border
        if (_screenBorderImage != null)
            _screenBorderImage.color = new Color(_damageColor.r, _damageColor.g, _damageColor.b, 0f);
    }

    private void CreateScreenBorder()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("HealthEffectCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Ensure it's on top
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create border image
        GameObject borderObj = new GameObject("ScreenBorder");
        borderObj.transform.SetParent(canvas.transform, false);

        _screenBorderImage = borderObj.AddComponent<Image>();
        _screenBorderImage.raycastTarget = false; // Don't block input

        // Create border texture programmatically
        Texture2D borderTexture = CreateBorderTexture();
        _screenBorderImage.sprite = Sprite.Create(borderTexture,
            new Rect(0, 0, borderTexture.width, borderTexture.height),
            new Vector2(0.5f, 0.5f), 100);

        // Setup RectTransform to cover entire screen
        RectTransform rectTransform = borderObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // Set initial color
        _screenBorderImage.color = new Color(_damageColor.r, _damageColor.g, _damageColor.b, 0f);
    }

    private Texture2D CreateBorderTexture()
    {
        int width = 256;
        int height = 256;
        int borderThickness = 50;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Create vignette/border effect
                float distFromLeft = x;
                float distFromRight = width - x;
                float distFromTop = y;
                float distFromBottom = height - y;

                float minDist = Mathf.Min(distFromLeft, distFromRight, distFromTop, distFromBottom);

                if (minDist < borderThickness)
                {
                    float alpha = 1f - (minDist / borderThickness);
                    alpha = Mathf.Pow(alpha, 2f); // Smooth gradient
                    pixels[y * width + x] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    pixels[y * width + x] = new Color(1, 1, 1, 0);
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void OnHealthChanged()
    {
        if (_player == null) return;

        int currentHealth = _player.Health;
        float healthPercent = (currentHealth / (float)MAX_HEALTH) * 100f;

        // Flash red when taking damage
        if (currentHealth < _lastHealth)
        {
            if (_damageFlashCoroutine != null)
                StopCoroutine(_damageFlashCoroutine);
            _damageFlashCoroutine = StartCoroutine(DamageFlash());
        }

        _lastHealth = currentHealth;

        // Update heartbeat based on health
        if (_heartbeatCoroutine != null)
            StopCoroutine(_heartbeatCoroutine);

        if (healthPercent <= _criticalHealthThreshold)
        {
            // Critical health - very fast heartbeat
            _heartbeatCoroutine = StartCoroutine(HeartbeatEffect(_criticalHeartbeatSpeed, 0.6f));
        }
        else if (healthPercent <= _lowHealthThreshold)
        {
            // Low health - fast heartbeat
            _heartbeatCoroutine = StartCoroutine(HeartbeatEffect(_fastHeartbeatSpeed, 0.4f));
        }
        else if (healthPercent <= 50f)
        {
            // Half health - normal heartbeat
            _heartbeatCoroutine = StartCoroutine(HeartbeatEffect(_normalHeartbeatSpeed, 0.2f));
        }
        // No effect above 50% health
    }

    private void OnInfectionChanged(bool infected)
    {
        _isInfected = infected;

        if (infected)
        {
            // Add purple tint when infected
            StartCoroutine(InfectionPulse());
        }
    }

    private IEnumerator DamageFlash()
    {
        // Quick flash when taking damage
        float flashDuration = 0.3f;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.7f, _currentAlpha, elapsed / flashDuration);

            Color color = _isInfected ? _infectionColor : _damageColor;
            if (_screenBorderImage != null)
                _screenBorderImage.color = new Color(color.r, color.g, color.b, alpha);

            yield return null;
        }
    }

    private IEnumerator HeartbeatEffect(float speed, float maxAlpha)
    {
        while (true)
        {
            // Heartbeat cycle
            float beatDuration = 1f / speed;
            float elapsed = 0f;

            // First beat (systole)
            while (elapsed < beatDuration * 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (beatDuration * 0.3f);
                float curveValue = _heartbeatCurve.Evaluate(t);
                _currentAlpha = Mathf.Lerp(0f, maxAlpha, curveValue);

                Color color = _isInfected ? _infectionColor : _damageColor;
                if (_screenBorderImage != null)
                    _screenBorderImage.color = new Color(color.r, color.g, color.b, _currentAlpha);

                yield return null;
            }

            // Quick fade
            elapsed = 0f;
            while (elapsed < beatDuration * 0.1f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (beatDuration * 0.1f);
                _currentAlpha = Mathf.Lerp(maxAlpha, maxAlpha * 0.3f, t);

                Color color = _isInfected ? _infectionColor : _damageColor;
                if (_screenBorderImage != null)
                    _screenBorderImage.color = new Color(color.r, color.g, color.b, _currentAlpha);

                yield return null;
            }

            // Second beat (diastole) - smaller
            elapsed = 0f;
            while (elapsed < beatDuration * 0.2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (beatDuration * 0.2f);
                float curveValue = _heartbeatCurve.Evaluate(t);
                _currentAlpha = Mathf.Lerp(maxAlpha * 0.3f, maxAlpha * 0.6f, curveValue);

                Color color = _isInfected ? _infectionColor : _damageColor;
                if (_screenBorderImage != null)
                    _screenBorderImage.color = new Color(color.r, color.g, color.b, _currentAlpha);

                yield return null;
            }

            // Rest period
            elapsed = 0f;
            while (elapsed < beatDuration * 0.4f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (beatDuration * 0.4f);
                _currentAlpha = Mathf.Lerp(maxAlpha * 0.6f, 0f, t);

                Color color = _isInfected ? _infectionColor : _damageColor;
                if (_screenBorderImage != null)
                    _screenBorderImage.color = new Color(color.r, color.g, color.b, _currentAlpha);

                yield return null;
            }
        }
    }

    private IEnumerator InfectionPulse()
    {
        // Slow pulse effect when infected
        while (_isInfected)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnDestroy()
    {
        if (_player != null)
        {
            _player.HealthChanged -= OnHealthChanged;
            _player.IsInfectedChanged -= OnInfectionChanged;
        }

        if (_heartbeatCoroutine != null)
            StopCoroutine(_heartbeatCoroutine);

        if (_damageFlashCoroutine != null)
            StopCoroutine(_damageFlashCoroutine);
    }
}
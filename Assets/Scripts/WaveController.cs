using System.Collections;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    [System.Serializable]
    public struct ZombiePreset
    {
        public Zombie Zombie;
        public int Chance;
    }

    [System.Serializable]
    public struct WavePreset
    {
        public int TimeLeft;
        public float SpawnInterval;
        public int SpawnCount;
        public ZombiePreset[] Zombies;
    }

    [SerializeField] private WavePreset[] _wavePresets;
    [SerializeField] private GameTimer _gameTimer;
    [SerializeField] private GameObject _vaccinePrefab;

    // Map boundaries coordinates
    private const float MAP_MIN_X = -70f;
    private const float MAP_MAX_X = 70f;
    private const float MAP_MIN_Y = -70f;
    private const float MAP_MAX_Y = 70f;
    private const float SPAWN_BUFFER = 5f;

    // Wave progression fields
    private bool _wave2Started, _wave3Started;
    private float _difficultyMultiplier = 1f;
    private float _spawnCountMultiplier = 1f;
    private float _timer;

    // Optional references
    private TutorialManager _tutorialManager;
    private UIController _uiController;
    private Collider2D[] _worldColliders;
    private Transform _playerTransform;

    private void Start()
    {
        _worldColliders = GameObject.FindObjectsOfType<Collider2D>();

        // Find optional components
        _tutorialManager = FindObjectOfType<TutorialManager>();
        _uiController = FindObjectOfType<UIController>();

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;

        StartCoroutine(SpawnVaccineRoutine());

        if (_tutorialManager != null)
        {
            StartCoroutine(MonitorTutorialEnd());
        }

        Debug.Log($"WaveController started with map bounds: X({MAP_MIN_X} to {MAP_MAX_X}), Y({MAP_MIN_Y} to {MAP_MAX_Y})");
    }

    private IEnumerator MonitorTutorialEnd()
    {
        // Wait for tutorial to be active
        while (_tutorialManager != null && !_tutorialManager.IsTutorialActive())
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Wait for tutorial to end
        while (_tutorialManager != null && _tutorialManager.IsTutorialActive())
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Tutorial just ended - refresh all existing zombies
        yield return new WaitForSeconds(0.5f);
        RefreshAllZombies();
    }

    private void RefreshAllZombies()
    {
        // Find all zombies and ensure they're killable after tutorial
        Zombie[] allZombies = FindObjectsOfType<Zombie>();
        foreach (var zombie in allZombies)
        {
            if (zombie != null)
            {
                // Toggle collider to refresh state
                var collider = zombie.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                    collider.enabled = true;
                }

                Debug.Log($"Refreshed zombie at {zombie.transform.position} to ensure it's killable");
            }
        }
    }

    private void Update()
    {
        // Skip during tutorial
        if (_tutorialManager != null && _tutorialManager.IsTutorialActive())
            return;

        if (_gameTimer.TimeLeft <= 0) return;

        // Check for wave progression milestones
        CheckWaveProgression();

        // Spawn logic with difficulty multiplier
        WavePreset? activePreset = GetActivePreset();
        if (activePreset == null)
        {
            Debug.LogWarning("No active wave preset found!");
            return;
        }

        _timer += Time.deltaTime;

        // Apply difficulty multiplier to spawn interval
        float adjustedSpawnInterval = activePreset.Value.SpawnInterval * _difficultyMultiplier;

        if (_timer >= adjustedSpawnInterval)
        {
            _timer = 0f;
            int spawnCount = Mathf.RoundToInt(activePreset.Value.SpawnCount * _spawnCountMultiplier);

            Debug.Log($"Spawning {spawnCount} zombies (Wave {GetCurrentWave()}, Interval: {adjustedSpawnInterval:F2}s)");

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnZombie(activePreset.Value.Zombies);
            }
        }
    }

    private void CheckWaveProgression()
    {
        // Wave 2 at 8 minutes (480 seconds)
        if (_gameTimer.TimeLeft <= 480f && !_wave2Started)
        {
            _wave2Started = true;
            _difficultyMultiplier = 0.8f; // Spawn 20% faster
            _spawnCountMultiplier = 1.2f; // 20% more zombies
            Debug.Log("WAVE 2 STARTED - Difficulty Increased!");
            ShowWaveNotification("WAVE 2 INCOMING!");
        }

        // Wave 3 at 5 minutes (300 seconds)
        if (_gameTimer.TimeLeft <= 300f && !_wave3Started)
        {
            _wave3Started = true;
            _difficultyMultiplier = 0.6f; // Spawn 40% faster
            _spawnCountMultiplier = 1.5f; // 50% more zombies
            Debug.Log("FINAL WAVE STARTED - Maximum Difficulty!");
            ShowWaveNotification("FINAL WAVE - SURVIVE!");
        }
    }

    private void ShowWaveNotification(string message)
    {
        Debug.Log($"Wave Notification: {message}");
        StartCoroutine(ShowTemporaryNotification(message));
    }

    private IEnumerator ShowTemporaryNotification(string message)
    {
        GameObject notificationObj = new GameObject("WaveNotification");
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas != null)
        {
            notificationObj.transform.SetParent(canvas.transform, false);

            var text = notificationObj.AddComponent<UnityEngine.UI.Text>();
            text.text = message;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 48;
            text.color = Color.red;
            text.alignment = TextAnchor.MiddleCenter;

            RectTransform rect = notificationObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.7f);
            rect.anchorMax = new Vector2(0.5f, 0.7f);
            rect.sizeDelta = new Vector2(600, 100);
            rect.anchoredPosition = Vector2.zero;

            // Fade animation
            float duration = 3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f;

                if (elapsed < 0.5f)
                    alpha = elapsed / 0.5f;
                else if (elapsed > duration - 0.5f)
                    alpha = (duration - elapsed) / 0.5f;

                text.color = new Color(1f, 0f, 0f, alpha);
                yield return null;
            }
        }

        Destroy(notificationObj);
    }

    private void SpawnZombie(ZombiePreset[] zombiePresets)
    {
        if (zombiePresets == null || zombiePresets.Length == 0)
        {
            Debug.LogError("No zombie presets available!");
            return;
        }

        Vector3 spawnPosition = GetSpawnPositionAroundCamera();

        // Validate position
        if (!IsValidSpawnPosition(spawnPosition))
        {
            for (int i = 0; i < 5; i++)
            {
                spawnPosition = GetSpawnPositionAroundCamera();
                if (IsValidSpawnPosition(spawnPosition))
                    break;
            }
        }

        // Spawn the zombie
        Zombie zombiePrefab = ChooseZombiePrefab(zombiePresets);
        if (zombiePrefab != null)
        {
            GameObject zombie = Instantiate(zombiePrefab.gameObject, spawnPosition, Quaternion.identity);
            Debug.Log($"Spawned zombie at {spawnPosition}");
        }
    }

    private Vector3 GetSpawnPositionAroundCamera()
    {
        Camera cam = Camera.main;
        Vector3 cameraPos = cam.transform.position;

        // Get camera boundaries in world space
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float camLeft = cameraPos.x - camWidth / 2f;
        float camRight = cameraPos.x + camWidth / 2f;
        float camTop = cameraPos.y + camHeight / 2f;
        float camBottom = cameraPos.y - camHeight / 2f;

        // Spawn just outside camera view but within map bounds
        int side = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;

        switch (side)
        {
            case 0: // Left side
                spawnPos = new Vector3(
                    Mathf.Max(camLeft - SPAWN_BUFFER, MAP_MIN_X + 2f),
                    Random.Range(Mathf.Max(camBottom, MAP_MIN_Y + 2f), Mathf.Min(camTop, MAP_MAX_Y - 2f)),
                    0f
                );
                break;

            case 1: // Right side
                spawnPos = new Vector3(
                    Mathf.Min(camRight + SPAWN_BUFFER, MAP_MAX_X - 2f),
                    Random.Range(Mathf.Max(camBottom, MAP_MIN_Y + 2f), Mathf.Min(camTop, MAP_MAX_Y - 2f)),
                    0f
                );
                break;

            case 2: // Top side
                spawnPos = new Vector3(
                    Random.Range(Mathf.Max(camLeft, MAP_MIN_X + 2f), Mathf.Min(camRight, MAP_MAX_X - 2f)),
                    Mathf.Min(camTop + SPAWN_BUFFER, MAP_MAX_Y - 2f),
                    0f
                );
                break;

            case 3: // Bottom side
                spawnPos = new Vector3(
                    Random.Range(Mathf.Max(camLeft, MAP_MIN_X + 2f), Mathf.Min(camRight, MAP_MAX_X - 2f)),
                    Mathf.Max(camBottom - SPAWN_BUFFER, MAP_MIN_Y + 2f),
                    0f
                );
                break;
        }

        // Ensure spawn position is within map bounds
        spawnPos.x = Mathf.Clamp(spawnPos.x, MAP_MIN_X + 2f, MAP_MAX_X - 2f);
        spawnPos.y = Mathf.Clamp(spawnPos.y, MAP_MIN_Y + 2f, MAP_MAX_Y - 2f);

        // Don't spawn too close to player
        if (_playerTransform != null)
        {
            float distToPlayer = Vector3.Distance(spawnPos, _playerTransform.position);
            if (distToPlayer < 5f)
            {
                Vector3 dirFromPlayer = (spawnPos - _playerTransform.position).normalized;
                spawnPos = _playerTransform.position + dirFromPlayer * 8f;

                // Re-clamp to map bounds
                spawnPos.x = Mathf.Clamp(spawnPos.x, MAP_MIN_X + 2f, MAP_MAX_X - 2f);
                spawnPos.y = Mathf.Clamp(spawnPos.y, MAP_MIN_Y + 2f, MAP_MAX_Y - 2f);
            }
        }

        return spawnPos;
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        // Check map bounds
        if (position.x < MAP_MIN_X || position.x > MAP_MAX_X ||
            position.y < MAP_MIN_Y || position.y > MAP_MAX_Y)
        {
            return false;
        }

        // Check for wall collisions
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(position, 0.5f);
        foreach (var col in overlaps)
        {
            if (col.isTrigger) continue;
            if (col.CompareTag("Zombie")) continue;
            if (col.GetComponent<Zombie>() != null) continue;

            return false;
        }

        return true;
    }

    private Zombie ChooseZombiePrefab(ZombiePreset[] zombiePresets)
    {
        if (zombiePresets == null || zombiePresets.Length == 0)
            return null;

        if (zombiePresets.Length == 1)
            return zombiePresets[0].Zombie;

        int totalChance = 0;
        foreach (var zp in zombiePresets)
            totalChance += zp.Chance;

        if (totalChance == 0)
            return zombiePresets[0].Zombie;

        int roll = Random.Range(0, totalChance);
        int cumulative = 0;

        foreach (var zp in zombiePresets)
        {
            cumulative += zp.Chance;
            if (roll < cumulative)
                return zp.Zombie;
        }

        return zombiePresets[0].Zombie;
    }

    private WavePreset? GetActivePreset()
    {
        if (_wavePresets == null || _wavePresets.Length == 0)
            return null;

        foreach (var preset in _wavePresets)
        {
            if (_gameTimer.TimeLeft >= preset.TimeLeft)
                return preset;
        }

        return _wavePresets[_wavePresets.Length - 1];
    }

    private IEnumerator SpawnVaccineRoutine()
    {
        yield return new WaitForSeconds(5f); // Initial delay

        while (true)
        {
            yield return new WaitForSeconds(15f);

            Vector3 spawnPos = GetSpawnPositionAroundCamera();

            if (IsValidSpawnPosition(spawnPos))
            {
                Instantiate(_vaccinePrefab, spawnPos, Quaternion.identity);
                Debug.Log($"Spawned vaccine at {spawnPos}");
            }
        }
    }

    public int GetCurrentWave()
    {
        if (_wave3Started) return 3;
        if (_wave2Started) return 2;
        return 1;
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WaveController : MonoBehaviour
{
    [SerializeField] private DroppableItem _vaccinePrefab;
    [SerializeField] private GameObject _worldRoot;
    [SerializeField] private float _spawnInterval = 3f;   // time between spawns
    [SerializeField] private float _spawnMargin = 1f;     // how far offscreen to spawn
    
    [SerializeField] private GameTimer _gameTimer;
    [SerializeField] private WavePreset[] _wavePresets;
    
    
    private Camera _cam;
    private Transform _player;
    private Collider2D _spawnCollider;
    private Bounds _spawnBounds;
    private float _timer;
    private Collider2D[] _worldColliders;

    private void Start()
    {
        _cam = Camera.main;
        _player = FindObjectOfType<Player>().transform;
        _spawnCollider = GetComponent<Collider2D>();
        _spawnCollider.enabled = true;
        _spawnBounds = _spawnCollider.bounds;
        _spawnCollider.enabled = false;
        
        _worldColliders = _worldRoot.GetComponentsInChildren<Collider2D>();
        _timer = _spawnInterval;

        //StartCoroutine(SpawnVaccineRoutine());
    }

    private void Update()
    {
        if (_gameTimer.TimeLeft <= 0) return;

        WavePreset? activePreset = GetActivePreset();
        if (activePreset == null) return;

        _timer += Time.deltaTime;

        if (_timer >= activePreset.Value.SpawnInterval)
        {
            _timer = 0f;

            for (int i = 0; i < activePreset.Value.SpawnCount; i++)
                SpawnZombie(activePreset.Value.Zombies);
        }
    }

    private void SpawnZombie(ZombiePreset[] zombiePresets)
    {
        if (_player == null) return;

        const int maxAttempts = 10;
        Vector3 spawnPos = Vector3.zero;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Pick a side of the screen: 0=left, 1=right, 2=top, 3=bottom
            int side = Random.Range(0, 4);

            switch (side)
            {
                case 0: // left
                    spawnPos = _cam.ViewportToWorldPoint(new Vector3(0, Random.value, _cam.nearClipPlane));
                    spawnPos.x -= _spawnMargin;
                    break;
                case 1: // right
                    spawnPos = _cam.ViewportToWorldPoint(new Vector3(1, Random.value, _cam.nearClipPlane));
                    spawnPos.x += _spawnMargin;
                    break;
                case 2: // top
                    spawnPos = _cam.ViewportToWorldPoint(new Vector3(Random.value, 1, _cam.nearClipPlane));
                    spawnPos.y += _spawnMargin;
                    break;
                case 3: // bottom
                    spawnPos = _cam.ViewportToWorldPoint(new Vector3(Random.value, 0, _cam.nearClipPlane));
                    spawnPos.y -= _spawnMargin;
                    break;
            }

            spawnPos.z = 0f;

            // Check spawn is inside play area (_spawnBounds)
            if (!_spawnBounds.Contains(new Vector3(spawnPos.x, spawnPos.y, _spawnBounds.center.z))) continue;

            // Check against world colliders
            bool insideCollider = false;
            foreach (var col in _worldColliders)
            {
                if (col.isTrigger) continue;
                if (col.OverlapPoint(spawnPos))
                {
                    insideCollider = true;
                    break;
                }
            }

            if (!insideCollider)
            {
                Zombie prefab = ChooseZombiePrefab(zombiePresets);
                Instantiate(prefab, spawnPos, Quaternion.identity);
                return;
            }
        }

        Debug.LogWarning("Failed to find valid spawn position after max attempts.");
    }

    private Zombie ChooseZombiePrefab(ZombiePreset[] zombiePresets)
    {
        int totalChance = 0;
        foreach (var zp in zombiePresets)
            totalChance += zp.Chance;

        int roll = Random.Range(0, totalChance);
        int cumulative = 0;

        foreach (var zp in zombiePresets)
        {
            cumulative += zp.Chance;
            if (roll < cumulative)
                return zp.Zombie;
        }

        // fallback
        return zombiePresets[0].Zombie;
    }

    private WavePreset? GetActivePreset()
    {
        foreach (var preset in _wavePresets)
        {
            if (_gameTimer.TimeLeft >= preset.TimeLeft)
                return preset;
        }
        return null;
    }

    private IEnumerator SpawnVaccineRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(15f);

            const int maxAttempts = 10;
            Vector2 worldPos = Vector2.zero;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Randomly pick which side of the screen (0=left, 1=right, 2=top, 3=bottom)
                int side = Random.Range(0, 4);
                var offset = Random.Range(1.1f, 3f); // how far outside the screen (1.0 = edge, >1 = offscreen)
                Vector2 spawnPos = Vector2.zero;

                switch (side)
                {
                    case 0: // Left
                        spawnPos = new Vector2(-offset, Random.value);
                        break;
                    case 1: // Right
                        spawnPos = new Vector2(offset, Random.value);
                        break;
                    case 2: // Top
                        spawnPos = new Vector2(Random.value, offset);
                        break;
                    case 3: // Bottom
                        spawnPos = new Vector2(Random.value, -offset);
                        break;
                }

                // Convert from viewport coords to world space
                worldPos = Camera.main.ViewportToWorldPoint(spawnPos);

                // Check against colliders
                bool insideCollider = false;
                foreach (var col in _worldColliders)
                {
                    if (col.isTrigger) continue;
                    if (col.OverlapPoint(worldPos))
                    {
                        insideCollider = true;
                        break;
                    }

                    if (col.OverlapPoint(worldPos + Vector2.up * 0.35f))
                    {
                        insideCollider = true;
                        break;
                    } // quick check with radius

                    if (col.OverlapPoint(worldPos + Vector2.right * 0.35f))
                    {
                        insideCollider = true;
                        break;
                    }

                    if (col.OverlapPoint(worldPos - Vector2.up * 0.35f))
                    {
                        insideCollider = true;
                        break;
                    }

                    if (col.OverlapPoint(worldPos - Vector2.right * 0.35f))
                    {
                        insideCollider = true;
                        break;
                    }
                }

                if (!insideCollider)
                {
                    Instantiate(_vaccinePrefab, worldPos, Quaternion.identity);
                    break;
                }
            }
        }
        // ReSharper disable once IteratorNeverReturns
    }

    [Serializable]
    private struct WavePreset
    {
        public int TimeLeft;
        public float SpawnInterval;
        public int SpawnCount;
        public ZombiePreset[] Zombies;
    }

    [Serializable]
    private struct ZombiePreset
    {
        public Zombie Zombie;
        public int Chance;
    }
}
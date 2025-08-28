using UnityEngine;

public class FlamethrowerWeapon : MonoBehaviour
{
    [Header("Flamethrower Settings")]
    [SerializeField] private int _damagePerTick = 2;
    [SerializeField] private float _damageTickRate = 0.1f; // Damage every 0.1 seconds
    [SerializeField] private float _range = 3f;
    [SerializeField] private float _spreadAngle = 30f; // Cone spread in degrees
    [SerializeField] private LayerMask _targetLayers = -1; // Default to all layers
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject _flameEffectPrefab;
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private AudioClip _flameSound;
    [SerializeField] private AudioClip _igniteSound;
    
    [Header("Status")]
    [SerializeField] private bool _isActive = false;
    
    private AudioSource _audioSource;
    private float _nextDamageTime;
    private GameObject _currentFlameEffect;
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source for flamethrower
        _audioSource.loop = true;
        _audioSource.volume = 0.6f;
    }
    
    public void StartFlamethrower()
    {
        if (_isActive) return;
        
        _isActive = true;
        _nextDamageTime = Time.time;
        
        // Start flame effect
        if (_flameEffectPrefab != null)
        {
            _currentFlameEffect = Instantiate(_flameEffectPrefab, _muzzlePoint.position, _muzzlePoint.rotation, _muzzlePoint);
        }
        
        // Start flame sound
        if (_flameSound != null && _audioSource != null)
        {
            _audioSource.clip = _flameSound;
            _audioSource.Play();
        }
        
        // Play ignite sound
        if (_igniteSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_igniteSound, 0.8f);
        }
    }
    
    public void StopFlamethrower()
    {
        if (!_isActive) return;
        
        _isActive = false;
        
        // Stop flame effect
        if (_currentFlameEffect != null)
        {
            Destroy(_currentFlameEffect);
            _currentFlameEffect = null;
        }
        
        // Stop flame sound
        if (_audioSource != null)
        {
            _audioSource.Stop();
        }
    }
    
    private void Update()
    {
        if (!_isActive) return;
        
        // Deal damage at tick rate
        if (Time.time >= _nextDamageTime)
        {
            DealFlameDamage();
            _nextDamageTime = Time.time + _damageTickRate;
        }
    }
    
    private void DealFlameDamage()
    {
        Vector2 direction = _muzzlePoint.right;
        Vector2 origin = _muzzlePoint.position;
        
        // Cast multiple rays in a cone pattern
        int rayCount = 8; // Number of rays to cast
        float angleStep = _spreadAngle / (rayCount - 1);
        float startAngle = -_spreadAngle / 2f;
        
        for (int i = 0; i < rayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 rayDirection = RotateVector2(direction, currentAngle);
            
            RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, _range, _targetLayers);
            
            if (hit.collider != null)
            {
                // Check if we hit a zombie
                Zombie zombie = hit.collider.GetComponent<Zombie>();
                if (zombie != null && zombie.Health > 0)
                {
                    zombie.TakeDamage(_damagePerTick);
                    
                    // Apply burn effect (optional)
                    // zombie.ApplyBurnEffect();
                }
                
                // Visual feedback - you can add flame particles at hit points
                // CreateFlameHitEffect(hit.point);
            }
        }
    }
    
    private Vector2 RotateVector2(Vector2 vector, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radian);
        float sin = Mathf.Sin(radian);
        
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }
    
    private void OnDrawGizmosSelected()
    {
        if (_muzzlePoint == null) return;
        
        // Draw flame cone
        Gizmos.color = Color.red;
        Vector3 origin = _muzzlePoint.position;
        Vector3 direction = _muzzlePoint.right;
        
        float halfSpread = _spreadAngle / 2f;
        Vector3 leftRay = Quaternion.Euler(0, 0, -halfSpread) * direction;
        Vector3 rightRay = Quaternion.Euler(0, 0, halfSpread) * direction;
        
        Gizmos.DrawRay(origin, leftRay * _range);
        Gizmos.DrawRay(origin, rightRay * _range);
        
        // Draw arc
        int segments = 20;
        float angleStep = _spreadAngle / segments;
        Vector3 prevPoint = origin + leftRay * _range;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfSpread + (angleStep * i);
            Vector3 ray = Quaternion.Euler(0, 0, angle) * direction;
            Vector3 currentPoint = origin + ray * _range;
            
            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
}

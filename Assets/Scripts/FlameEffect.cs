using UnityEngine;

public class FlameEffect : MonoBehaviour
{
    [Header("Flame Settings")]
    [SerializeField] private float _lifetime = 0.5f;
    [SerializeField] private float _fadeOutDuration = 0.2f;
    [SerializeField] private Vector2 _scaleRange = new Vector2(0.8f, 1.2f);
    [SerializeField] private float _flickerSpeed = 10f;
    [SerializeField] private float _flickerIntensity = 0.3f;
    
    [Header("Components")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private ParticleSystem _particleSystem;
    
    private float _startTime;
    private Vector3 _originalScale;
    private Color _originalColor;
    
    private void Start()
    {
        _startTime = Time.time;
        _originalScale = transform.localScale;
        _originalColor = _spriteRenderer != null ? _spriteRenderer.color : Color.white;
        
        // Randomize initial scale
        float randomScale = Random.Range(_scaleRange.x, _scaleRange.y);
        transform.localScale = _originalScale * randomScale;
        
        // Start particle system if available
        if (_particleSystem != null)
        {
            _particleSystem.Play();
        }
        
        // Destroy after lifetime
        Destroy(gameObject, _lifetime);
    }
    
    private void Update()
    {
        float elapsedTime = Time.time - _startTime;
        float lifeProgress = elapsedTime / _lifetime;
        
        // Flicker effect
        if (_spriteRenderer != null)
        {
            float flicker = 1f + Mathf.Sin(Time.time * _flickerSpeed) * _flickerIntensity;
            _spriteRenderer.color = _originalColor * flicker;
        }
        
        // Fade out near end of life
        if (lifeProgress > (1f - _fadeOutDuration / _lifetime))
        {
            float fadeProgress = (lifeProgress - (1f - _fadeOutDuration / _lifetime)) / (_fadeOutDuration / _lifetime);
            float alpha = 1f - fadeProgress;
            
            if (_spriteRenderer != null)
            {
                Color color = _spriteRenderer.color;
                color.a = alpha;
                _spriteRenderer.color = color;
            }
        }
    }
    
    // Call this when the flame hits something
    public void OnHit()
    {
        if (_particleSystem != null)
        {
            _particleSystem.Stop();
        }
        
        // Destroy immediately
        Destroy(gameObject);
    }
}

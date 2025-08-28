using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Zombie : LivingEntity
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _hitFlesh;
    [SerializeField] private AudioClip _deathSfx;
    [SerializeField] private AudioClip _infectionSound;
    [SerializeField] private AudioClip _vaccinePickupSound;
    [SerializeField] private AudioClip _weaponPickupSound;

    private Player _player;
    private AIPath _aiPath;
    private float _lastSearchPathTime;
    private TutorialManager _tutorialManager;

    protected override void Start()
    {
        base.Start();
        _aiPath = GetComponent<AIPath>();
        _tutorialManager = FindObjectOfType<TutorialManager>();

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        // Stop all zombie activity during tutorial
        if (_tutorialManager != null && _tutorialManager.IsTutorialActive())
        {
            // Disable AI movement during tutorial
            if (_aiPath != null)
            {
                _aiPath.canMove = false;
                _aiPath.canSearch = false;
            }
            return;
        }

        // Re-enable AI movement after tutorial
        if (_aiPath != null && (!_aiPath.canMove || !_aiPath.canSearch))
        {
            _aiPath.canMove = true;
            _aiPath.canSearch = true;
        }

        if (_player == null) return;

        // Update destination toward player
        _aiPath.destination = _player.transform.position;

        if (Time.time - _lastSearchPathTime > 2f)
        {
            _aiPath.SearchPath();
            _lastSearchPathTime = Time.time;
        }

        // Flip sprite based on movement direction
        Vector2 velocity = _aiPath.velocity;
        if (velocity.sqrMagnitude > 0.1f)
        {
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                _spriteRenderer.flipX = velocity.x < 0;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Don't damage player during tutorial
        if (_tutorialManager != null && _tutorialManager.IsTutorialActive())
        {
            return;
        }

        var player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(_damage);
            player.Infect();

            // Apply knockback
            Vector2 knockDir = (player.transform.position - transform.position).normalized;
            player.Knockback(knockDir, _knockbackForce);

            Debug.Log("Zombie hit player!");
        }
    }
    public override void TakeDamage(int damage)
    {
        // Make zombies invincible during tutorial
        if (_tutorialManager != null && _tutorialManager.IsTutorialActive())
        {
            // Play hit sound but don't take damage
            if (!_audioSource.isPlaying && _hitFlesh != null)
                _audioSource.PlayOneShot(_hitFlesh, Random.Range(0.8f, 1f));

            Debug.Log("Zombie is invincible during tutorial!");
            return; // Don't process damage
        }

        // Normal damage processing
        base.TakeDamage(damage);
        if (!_audioSource.isPlaying && _hitFlesh != null)
            _audioSource.PlayOneShot(_hitFlesh, Random.Range(0.8f, 1f));
    }
    protected override void Die()
    {
        _audioSource.PlayOneShot(_deathSfx, Random.Range(1.6f, 2f));
        _audioSource.transform.parent = transform.parent;
        Destroy(_audioSource.gameObject, _deathSfx.length);
        base.Die();
    }

    public void KnockBack(Vector2 direction, float force, float duration = 1f)
    {
        // Don't knockback during tutorial
        if (_tutorialManager != null && _tutorialManager.IsTutorialActive())
        {
            return;
        }

        _aiPath.canMove = false;
        _aiPath.canSearch = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }

        StartCoroutine(RestoreMovement(duration));
    }

    private IEnumerator RestoreMovement(float delay)
    {
        yield return new WaitForSeconds(delay);
        _aiPath.canMove = true;
        _aiPath.canSearch = true;
    }
}
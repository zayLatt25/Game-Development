using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private float _knockbackDuration = 0.3f;

    public bool CanDealDamage;
    private Collider2D _collider2d;

    private void Start()
    {
        _collider2d = GetComponent<Collider2D>();
        if (_collider2d == null)
        {
            Debug.LogError("MeleeWeapon requires a Collider2D!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other object has a LivingEntity (zombie, etc.)
        var target = other.GetComponent<LivingEntity>();
        if (target != null && target.Health > 0)
        {
            // Deal damage
            if (CanDealDamage)
            {
                target.TakeDamage(_damage);
            }

            // Knockback if target has Rigidbody2D
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                Vector2 knockBackDir = (other.transform.position - transform.position).normalized;
                // Check if the target is a Zombie specifically for knockback
                var zombie = target as Zombie;
                if (zombie != null)
                {
                    zombie.KnockBack(knockBackDir, _knockbackForce);
                }
            }
        }
    }
}
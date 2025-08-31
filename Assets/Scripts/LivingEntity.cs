

using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class LivingEntity : MonoBehaviour
{
    [Header("Stats")]
    // MODIFIED: Added [field: SerializeField]
    // This attribute tells Unity to show this property in the Inspector.
    [field: SerializeField] public int MaxHealth { get; protected set; } = 100;
    public int Health { get; private set; } = 100;
    
    public event Action HealthChanged;

    protected Rigidbody2D Rigidbody2D;

    protected virtual void Start()
    {
        Health = MaxHealth; 

        Rigidbody2D = GetComponent<Rigidbody2D>();
        Rigidbody2D.gravityScale = 0f;
        Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public virtual void TakeDamage(int damage)
    {
        Health = Mathf.Clamp(Health - damage, 0, MaxHealth);
        HealthChanged?.Invoke();
        if (Health <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"{name} died");
        Destroy(gameObject);
    }
}


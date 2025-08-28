using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 10;

    private Vector2 direction;

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;

        // Rotate bullet so its right (local +X) matches direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit a LivingEntity
        var entity = other.GetComponent<LivingEntity>();
        if (entity is Player) return;
        if (entity != null)
        {
            entity.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.isTrigger && other.gameObject.layer == LayerMask.NameToLayer("Obstacle")) // e.g. wall/obstacle
        {
            Destroy(gameObject);
        }
    }
}
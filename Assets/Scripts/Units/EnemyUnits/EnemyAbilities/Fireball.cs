using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 8f;
    public float explosionRadius = 2f;
    public float damage = 50f;
    public LayerMask targetLayer;
    public GameObject explosionEffect;

    private Vector2 direction;

    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Explode();
    }

    void Explode()
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, targetLayer);
        foreach (var hit in hits)
        {
            //var health = hit.GetComponent<HealthComponent>();
            //if (health != null)
            //    health.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
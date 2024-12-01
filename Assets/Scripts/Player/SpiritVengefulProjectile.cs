using UnityEngine;

public class SpiritVengefulProjectile : MonoBehaviour
{
    public float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.hurt(2);
            }
            Destroy(gameObject);
        }
    }
}

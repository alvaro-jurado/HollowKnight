using UnityEngine;

public class Orb : MonoBehaviour
{
    public float speed = 3f;
    private Transform target;
    public int damage = 1;

    public void Initialize(Transform target)
    {
        this.target = target;
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.hurt(damage);
            }

            Destroy(gameObject);
        }
    }
}

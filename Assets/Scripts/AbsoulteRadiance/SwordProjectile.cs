using UnityEngine;

public class SwordProjectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;

    private Vector2 direction;
    private Collider2D spikeCollider;

    // Método para definir la dirección
    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
    }

    private void Awake()
    {
        
        if (gameObject.name == "BurstLightBeam(Clone)")
        {
            spikeCollider = GetComponent<Collider2D>();
            spikeCollider.enabled = false;
        }
    }

    private void Update()
    {
        // Mueve el proyectil en la dirección global calculada
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Destruir el proyectil si sale de la pantalla o después de un tiempo
        Destroy(gameObject, 3f); // Ajusta el tiempo si es necesario
    }

    public void EnableCollider()
    {
        spikeCollider.enabled = true;
    }

    public void DisableCollider()
    {
        spikeCollider.enabled = false;
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

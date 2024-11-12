using UnityEngine;

public class SwordProjectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;

    private Vector2 direction;

    // Método para definir la dirección
    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
    }

    private void Update()
    {
        // Mueve el proyectil en la dirección global calculada
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Destruir el proyectil si sale de la pantalla o después de un tiempo
        Destroy(gameObject, 3f); // Ajusta el tiempo si es necesario
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Acciones al golpear al jugador (aplicar daño)
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.hurt(damage);
            }
            Destroy(gameObject);
        }
    }
}

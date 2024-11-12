using System.Collections;
using UnityEngine;

public class RadianceController : MonoBehaviour
{
    public int health = 18;
    public Transform[] teleportPoints;
    public GameObject swordProjectilePrefab;
    public GameObject beamPrefab;
    public float attackInterval = 2.0f;

    private Animator _animator;
    private bool isAlive = true;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        StartCoroutine(AttackPattern());
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        health -= damage;
        Debug.Log(health);
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isAlive = false;
        _animator.SetTrigger("Die");
        Destroy(gameObject, 2.0f);
    }

    private IEnumerator AttackPattern()
    {
        while (isAlive)
        {
            yield return new WaitForSeconds(attackInterval);

            if (health > 15)
            {
                Teleport();
                SwordBurstAttack();
            }
            else if (health > 11)
            {
                SwordBurstAttack();
                yield return new WaitForSeconds(1.0f);
                BeamAttack();
            }
           /* else if (health > 8)
            {
                // Fase 2: Sword Burst + Beams
                SwordBurstAttack();
                yield return new WaitForSeconds(1.0f);
                BeamAttack();
            }
            else if (health > 1)
            {
                // Fase 2: Sword Burst + Beams
                SwordBurstAttack();
                yield return new WaitForSeconds(1.0f);
                BeamAttack();
            }*/
            else
            {
                Teleport();
                yield return new WaitForSeconds(0.5f);
                SwordBurstAttack();
                yield return new WaitForSeconds(0.5f);
                BeamAttack();
            }
        }
    }

    private void Teleport()
    {
        int randomIndex = Random.Range(0, teleportPoints.Length);
        transform.position = teleportPoints[randomIndex].position;
        _animator.SetTrigger("Teleport");
    }

    private void SwordBurstAttack()
    {
        _animator.SetTrigger("SwordBurst");
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45.0f; // Genera 8 ángulos separados uniformemente.
            float radian = angle * Mathf.Deg2Rad; // Convierte el ángulo a radianes.

            // Calcula la dirección usando coseno y seno.
            Vector2 direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized; // Asegura direcciones normalizadas.

            // Instancia el proyectil con una rotación visual.
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject swordProjectile = Instantiate(swordProjectilePrefab, transform.position, rotation);

            SwordProjectile sword = swordProjectile.GetComponent<SwordProjectile>();
            if (sword != null)
            {
                // Asigna la dirección directamente al proyectil para que se mueva correctamente.
                sword.Initialize(direction);
                //Debug.Log($"Proyectil {i}: Dirección asignada = {direction}"); // Depuración
            }
        }
    }

    private void BeamAttack()
    {
        _animator.SetTrigger("Beam");
        Instantiate(beamPrefab, transform.position, Quaternion.identity);
    }

}

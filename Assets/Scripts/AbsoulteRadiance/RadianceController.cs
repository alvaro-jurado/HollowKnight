using System.Collections;
using UnityEngine;
using System;


public class RadianceController : MonoBehaviour
{
    public int health = 18;
    public int phase1HealthThreshold = 15;
    public int phase2HealthThreshold = 11;
    public int phase3HealthThreshold = 8;
    public int phase4HealthThreshold = 1;

    public Transform[] teleportPoints;
    public GameObject swordProjectilePrefab;
    public GameObject beamBurstPrefab;
    public GameObject swordRainPrefab;
    public GameObject orbPrefab;
    public GameObject spikePrefab;

    public float attackInterval = 1.5f;

    private Animator _animator;
    private bool isAlive = true;
    private bool isAttacking = false;
    private int currentPhase = 1;
    private int lastTeleportIndex = -1;

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
        else
        {
            UpdatePhase();
        }
    }

    private void UpdatePhase()
    {
        if (health > phase1HealthThreshold)
            currentPhase = 1;
        else if (health > phase2HealthThreshold)
            currentPhase = 2;
        else if (health > phase3HealthThreshold)
            currentPhase = 3;
        else if (health > phase4HealthThreshold)
            currentPhase = 4;
        else
            currentPhase = 5;
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
            if (!isAttacking)
            {
                isAttacking = true;
                yield return new WaitForSeconds(attackInterval);

                var phaseAttacks = GetAttacksForCurrentPhase();
                if (phaseAttacks.Length > 0)
                {                    
                    var attack = phaseAttacks[UnityEngine.Random.Range(0, phaseAttacks.Length)];
                    yield return StartCoroutine(attack.Invoke());
                }

                isAttacking = false;
                //yield return new WaitForSeconds(attackInterval);
            }
            else
            {
                yield return null;
            }
        }
    }

    private Func<IEnumerator>[] GetAttacksForCurrentPhase()
    {
        switch (currentPhase)
        {
            case 1:
                return new Func<IEnumerator>[] { SwordBurstAttack, BeamBurstAttack, Teleport };
            case 2:
                return new Func<IEnumerator>[] { SwordRainAttack, /*SpikeFloorAttack*/ };
            case 3:
                return new Func<IEnumerator>[] { SwordRainAttack };
            case 4:
                return new Func<IEnumerator>[] { () => SwordWallAttack(UnityEngine.Random.value > 0.5f), OrbAttack };
            case 5:
                return new Func<IEnumerator>[] { SwordRainAttack, OrbAttack, SpikeFloorAttack };
            default:
                return new Func<IEnumerator>[] { };
        }
    }

    private IEnumerator Teleport()
    {
        int randomIndex;

        do
        {
            randomIndex = UnityEngine.Random.Range(0, teleportPoints.Length);
        } while (randomIndex == lastTeleportIndex);

        lastTeleportIndex = randomIndex;
        transform.position = teleportPoints[randomIndex].position;
        _animator.SetTrigger("Teleport");
        yield return new WaitForSeconds(0.2f);
    }


    private IEnumerator SwordBurstAttack()
    {
        _animator.SetTrigger("SwordBurst");
        for (int i = 0; i < 12; i++)
        {
            float angle = i * 30.0f;
            float radian = angle * Mathf.Deg2Rad;
            
            Vector2 direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;

            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject swordProjectile = Instantiate(swordProjectilePrefab, transform.position, rotation);

            SwordProjectile sword = swordProjectile.GetComponent<SwordProjectile>();
            if (sword != null)
            {
                sword.Initialize(direction);
                //Debug.Log($"Proyectil {i}: Dirección asignada = {direction}");
            }
            
        }
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator BeamBurstAttack()
    {
        _animator.SetTrigger("BeamBurst");
        for (int burst = 0; burst < 3; burst++)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45.0f + (burst * 15.0f);
                Quaternion rotation = Quaternion.Euler(0, 0, angle);
                Instantiate(beamBurstPrefab, transform.position, rotation);
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    private IEnumerator SwordRainAttack()
    {
        float columnSpacing = 1.5f;

        for (int wave = 0; wave < (currentPhase == 3 ? int.MaxValue : 4); wave++)
        {
            for (int column = 0; column < 18; column++)
            {
                if (UnityEngine.Random.value > 0.3f)
                {                    
                    float offsetX = (column - 8.5f) * columnSpacing;
                    Vector2 spawnPosition = new Vector2(offsetX, transform.position.y + 5);

                    Quaternion rotation = Quaternion.Euler(0, 0, -90f);
                    GameObject swordProjectile = Instantiate(swordRainPrefab, spawnPosition, rotation);

                    SwordProjectile sword = swordProjectile.GetComponent<SwordProjectile>();
                    if (sword != null)
                    {
                        sword.Initialize(Vector2.down);
                    }
                    else
                    {
                        Debug.LogError("SwordProjectile no encontrado en swordRainPrefab.");
                    }
                }
            }
            yield return new WaitForSeconds(wave == 0 ? 2.5f : 2.0f);
        }
    }

    private IEnumerator SwordWallAttack(bool leftToRight)
    {
        float rowSpacing = 2.5f;
        int totalRows = 9;

        float startY = -((totalRows - 1) / 2) * rowSpacing;

        for (int wall = 0; wall < (currentPhase == 4 ? 2 : 4); wall++)
        {
            for (int row = 0; row < totalRows; row++)
            {
                float posY = startY + row * rowSpacing;
                float posX = leftToRight ? -15 : 5;

                Vector2 spawnPosition = new Vector2(posX, posY);

                Quaternion rotation = leftToRight
                    ? Quaternion.identity
                    : Quaternion.Euler(0, 180, 0);

                GameObject swordProjectile = Instantiate(swordProjectilePrefab, spawnPosition, rotation);

                SwordProjectile sword = swordProjectile.GetComponent<SwordProjectile>();
                if (sword != null)
                {
                    sword.Initialize(leftToRight ? Vector2.right : Vector2.left);
                }
            }

            yield return new WaitForSeconds(wall == 0 ? 2.0f : 1.5f);
        }
    }

    private IEnumerator WallOfLightAttack(bool leftToRight)
    {
        _animator.SetTrigger("WallOfLight");
        GameObject wall = Instantiate(beamBurstPrefab, transform.position, Quaternion.identity);
        wall.GetComponent<Rigidbody2D>().velocity = new Vector2(leftToRight ? 1 : -1, 0) * 5f;
        yield return new WaitForSeconds(3.5f);
    }

    private IEnumerator OrbAttack()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        for (int i = 0; i < 3; i++)
        {
            GameObject orb = Instantiate(orbPrefab, transform.position, Quaternion.identity);

            Orb orbScript = orb.GetComponent<Orb>();
            if (orbScript != null && player != null)
            {
                orbScript.Initialize(player.transform);
            }
            yield return new WaitForSeconds(1.65f);
        }
    }

    private IEnumerator SpikeFloorAttack()
    {
        Vector2 spikePosition = new Vector2(UnityEngine.Random.Range(-5, 5), -3);
        GameObject spikeGlow = Instantiate(spikePrefab, spikePosition, Quaternion.identity);
        yield return new WaitForSeconds(1.0f);
        spikeGlow.GetComponent<SpriteRenderer>().color = Color.red;
        Destroy(spikeGlow, 5.0f);
    }
}

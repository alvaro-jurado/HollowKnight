using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBehaviour : MonoBehaviour
{
    private Collider2D spikeCollider;
    public int damage = 1;

    private void Awake()
    {
        spikeCollider = GetComponent<Collider2D>();
        spikeCollider.enabled = false;
    }

    public void Start()
    {
        Destroy(gameObject, 1f);
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
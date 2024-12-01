using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerController : EnemyController
{
    public float shootInterval;
    public GameObject projectilePrefab;

    private bool isShooting;
    private bool isShootable;

    private Transform playerTransform;
    private Transform _transform;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        playerTransform = GlobalController.Instance.player.GetComponent<Transform>();
        _transform = gameObject.GetComponent<Transform>();
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _animator = gameObject.GetComponent<Animator>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        isShootable = true;
        isShooting = false;

        currentState = new Idle();
    }

    void Update()
    {
        playerEnemyDistanceVar = playerTransform.position.x - _transform.position.x;

        int direction = playerEnemyDistanceVar > 0 ? 1 : playerEnemyDistanceVar < 0 ? -1 : 0;

        if (direction != 0 && health > 0)
        {
            Vector3 newScale = _transform.localScale;
            newScale.x = direction;
            _transform.localScale = newScale;
        }

        if (!currentState.checkValid(this))
        {
            if (isShooting)
            {
                currentState = new Shooting();
            }
            else
            {
                currentState = new Idle();
            }

            isShooting = !isShooting;
        }

        if (health > 0)
            currentState.Execute(this);
    }

    public override float behaveInterval()
    {
        return shootInterval;
    }

    public override void hurt(int damage)
    {
        health = Math.Max(health - damage, 0);

        if (health == 0)
        {
            die();
            return;
        }

        Vector2 newVelocity = hurtRecoil;
        newVelocity.x *= _transform.localScale.x;

        _rigidbody.velocity = newVelocity;

        StartCoroutine(hurtCoroutine());
    }

    private IEnumerator hurtCoroutine()
    {
        yield return new WaitForSeconds(hurtRecoilTime);

        Vector2 newVelocity;
        newVelocity.x = 0;
        newVelocity.y = 0;
        _rigidbody.velocity = newVelocity;
    }

    protected override void die()
    {
        _animator.SetTrigger("isDead");

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        _rigidbody.bodyType = RigidbodyType2D.Dynamic;

        Vector2 newVelocity = Vector2.zero;
        _rigidbody.velocity = newVelocity;

        gameObject.layer = LayerMask.NameToLayer("EnemyDead");

        Vector2 newForce = new Vector2(
            _transform.localScale.x * deathForce.x,
            deathForce.y
        );
        _rigidbody.AddForce(newForce, ForceMode2D.Impulse);

        StartCoroutine(fadeCoroutine());
    }


    private IEnumerator fadeCoroutine()
    {

        while (destroyDelay > 0)
        {
            destroyDelay -= Time.deltaTime;

            if (_spriteRenderer.color.a > 0)
            {
                Color newColor = _spriteRenderer.color;
                newColor.a -= Time.deltaTime / destroyDelay;
                _spriteRenderer.color = newColor;
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    private void shootPlayer()
    {
        if (isShootable)
        {
            _animator.SetTrigger("attack");

            isShootable = false;

            Vector2 direction = playerTransform.position - _transform.position;
            StartCoroutine(shootPlayerCoroutine(direction, shootInterval));
        }
    }

    private IEnumerator shootPlayerCoroutine(Vector2 direction, float shootInterval)
    {
        yield return new WaitForSeconds(0.2f);

        Vector3 position = _transform.position;
        Quaternion rotation = _transform.rotation;
        GameObject projectileObj = Instantiate(projectilePrefab, position, rotation);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        projectile.direction = direction;
        projectile.trigger();

        yield return new WaitForSeconds(shootInterval);
        if (!isShootable)
            isShootable = true;
    }

    /* ----------------------------------------------------------------------------------- */

    public class Idle : State
    {
        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs > enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            // Don't do anything while being idle
        }
    }

    /* ----------------------------------------------------------------------------------- */
    public class Shooting : State
    {

        public override bool checkValid(EnemyController enemyController)
        {
            float playerEnemyDistanceAbs = Math.Abs(enemyController.playerEnemyDistance());
            return playerEnemyDistanceAbs <= enemyController.detectDistance;
        }

        public override void Execute(EnemyController enemyController)
        {
            GunnerController gunnerController = (GunnerController)enemyController;

            gunnerController.shootPlayer();
        }
    }
}

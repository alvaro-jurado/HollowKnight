using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public int health;
    public float moveSpeed;
    public float jumpSpeed;
    public int jumpLeft;
    public Vector2 climbJumpForce;
    public float fallSpeed;
    public float sprintSpeed;
    public float sprintTime;
    public float sprintInterval;
    public float attackInterval;

    public Color invulnerableColor;
    public Vector2 hurtRecoil;
    public float hurtTime;
    public float hurtRecoverTime;
    public Vector2 deathRecoil;
    public float deathDelay;

    public Vector2 attackUpRecoil;
    public Vector2 attackForwardRecoil;
    public Vector2 attackDownRecoil;

    public GameObject attackUpEffect;
    public GameObject attackForwardEffect;
    public GameObject attackDownEffect;

    public GameObject spiritVengefulProjectilePrefab;
    public float projectileSpeed;

    private bool isGrounded;
    private bool isClimb;
    private bool isSprintable;
    private bool isSprintReset;
    private bool isInputEnabled;
    private bool isFalling;
    private bool isAttackable;
    private bool isSpiritVengeful;

    private float climbJumpDelay = 0.2f;
    private float attackEffectLifeTime = 0.05f;

    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private Transform _transform;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;

    private void Start() {
        isInputEnabled = true;
        isSprintReset = true;
        isAttackable = true;
        isSpiritVengeful = true;

        _animator = gameObject.GetComponent<Animator>();
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _transform = gameObject.GetComponent<Transform>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _boxCollider = gameObject.GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        updatePlayerState();
        if (isInputEnabled)
        {
            move();
            jumpControl();
            fallControl();
            sprintControl();
            attackControl();
            spiritVengefulControl();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Wall" && !isGrounded)
        {
            _rigidbody.gravityScale = 0;

            Vector2 newVelocity;
            newVelocity.x = 0;
            newVelocity.y = -2;

            _rigidbody.velocity = newVelocity;

            isClimb = true;
            _animator.SetBool("IsClimb", true);

            isSprintable = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Bench")
        {
            SceneManager.LoadScene("Radiance");
        }
        if (other.tag == "GodhomeEntrance")
        {
            SceneManager.LoadScene("Godhome");
        }
        if (other.tag == "ResetGround")
        {
            hurt(100);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.tag == "Wall" && isFalling && !isClimb)
        {
            OnCollisionEnter2D(collision);
        }
    }

    public void hurt(int damage)
    {
        if (gameObject.layer == LayerMask.NameToLayer("PlayerInvulnerable"))
        {
            Debug.Log("El jugador es invulnerable y no puede recibir daño.");
            return;
        }

        health = Math.Max(health - damage, 0);
        Debug.Log("Player hp: " + health);

        if (health == 0)
        {
            die();
            return;
        }

        gameObject.layer = LayerMask.NameToLayer("PlayerInvulnerable");
        _animator.SetTrigger("IsHurt");

        Vector2 newVelocity;
        newVelocity.x = 0;
        newVelocity.y = 0;
        _rigidbody.velocity = newVelocity;

        _spriteRenderer.color = invulnerableColor;

        Vector2 newForce;
        newForce.x = -_transform.localScale.x * hurtRecoil.x;
        newForce.y = hurtRecoil.y;
        _rigidbody.AddForce(newForce, ForceMode2D.Impulse);

        isInputEnabled = false;

        StartCoroutine(recoverFromHurtCoroutine());
    }

    private IEnumerator recoverFromHurtCoroutine()
    {
        yield return new WaitForSeconds(hurtTime);
        isInputEnabled = true;
        yield return new WaitForSeconds(hurtRecoverTime);
        _spriteRenderer.color = Color.white;
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // exit climb state
        if (collision.collider.tag == "Wall")
        {
            isClimb = false;
            _animator.SetBool("IsClimb", false);

            _rigidbody.gravityScale = 1;
        }
    }

    private void updatePlayerState()
    {
        isGrounded = checkGrounded();
        _animator.SetBool("IsGround", isGrounded);

        float verticalVelocity = _rigidbody.velocity.y;
        _animator.SetBool("IsDown", verticalVelocity < 0);

        if (isGrounded )
        {
            _animator.SetBool("IsJump", false);
            _animator.ResetTrigger("IsJumpFirst");
            _animator.ResetTrigger("IsJumpSecond");
            _animator.SetBool("IsDown", false);

            jumpLeft = 2;
            isClimb = false;
            isSprintable = true;
        }
        else if(isClimb)
        {
            jumpLeft = 1;
        }
    }

    private void move()
    {
        float horizontalMovement = Input.GetAxis("Horizontal") * moveSpeed;

        Vector2 newVelocity;
        newVelocity.x = horizontalMovement;
        newVelocity.y = _rigidbody.velocity.y;
        _rigidbody.velocity = newVelocity;

        if (!isClimb)
        {
            float moveDirection = -_transform.localScale.x * horizontalMovement;

            if (moveDirection < 0)
            {
                Vector3 newScale;
                newScale.x = horizontalMovement < 0 ? 1 : -1;
                newScale.y = 1;
                newScale.z = 1;

                _transform.localScale = newScale;

                if (isGrounded)
                {
                    _animator.SetTrigger("IsRotate");
                }
            }
            else if (moveDirection > 0)
            {
                _animator.SetBool("IsRun", true);
            }
        }

        if (Input.GetAxis("Horizontal") == 0)
        {
            _animator.SetTrigger("stopTrigger");
            _animator.ResetTrigger("IsRotate");
            _animator.SetBool("IsRun", false);
        }
        else
        {
            _animator.ResetTrigger("stopTrigger");
        }
    }

    private void jumpControl()
    {
        if (!Input.GetButtonDown("Jump"))
            return;

        if (isClimb)
            climbJump();
        else if (jumpLeft > 0)
            jump();
    }

    private void fallControl()
    {
        if (Input.GetButtonUp("Jump") && !isClimb)
        {
            isFalling = true;
            fall();
        } else
        {
            isFalling = false;
        }
    }

    private void sprintControl()
    {
        if (Input.GetKeyDown(KeyCode.K) && isSprintable && isSprintReset)
            sprint();
    }

    private void attackControl()
    {
        if (Input.GetKeyDown(KeyCode.J) && !isClimb && isAttackable)
            attack();
    }


    private void spiritVengefulControl()
    {
        if (Input.GetKeyDown(KeyCode.H) && !isClimb && isAttackable && isSpiritVengeful)
        {
            isSpiritVengeful = false;
            //_animator.SetTrigger("IsSpiritVengeful");
            ShootSpiritVengefulProjectile();
            StartCoroutine(ActivateSpiritVengeful());
        }
    }

    private void ShootSpiritVengefulProjectile()
    {
        GameObject projectile = Instantiate(spiritVengefulProjectilePrefab, _transform.position, Quaternion.identity);

        Vector2 direction = _transform.localScale.x < 0 ? Vector2.right : Vector2.left;
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            projectileRb.velocity = direction * projectileSpeed;
        }

        if (direction == Vector2.right)
        {
            projectile.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            projectile.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }


    private IEnumerator ActivateSpiritVengeful()
    {
        yield return new WaitForSeconds(1f);
        isSpiritVengeful = false;

        //_animator.ResetTrigger("IsSpiritVengeful");
        yield return new WaitForSeconds(0.1f);
        isSpiritVengeful = true;
    }

    private void die()
    {
        _animator.SetTrigger("IsDead");

        isInputEnabled = false;

        Vector2 newVelocity;
        newVelocity.x = 0;
        newVelocity.y = 0;
        _rigidbody.velocity = newVelocity;

        _spriteRenderer.color = invulnerableColor;

        Vector2 newForce;
        newForce.x = -_transform.localScale.x * deathRecoil.x;
        newForce.y = deathRecoil.y;
        _rigidbody.AddForce(newForce, ForceMode2D.Impulse);

        StartCoroutine(deathCoroutine());
    }

    private IEnumerator deathCoroutine()
    {
        var material = _boxCollider.sharedMaterial;
        material.bounciness = 0.3f;
        material.friction = 0.3f;
        _boxCollider.enabled = false;
        _boxCollider.enabled = true;

        yield return new WaitForSeconds(deathDelay);

        material.bounciness = 0;
        material.friction = 0;
        SceneManager.LoadScene("MainMenu");
    }


    private bool checkGrounded()
    {
        Vector2 origin = _transform.position;

        float radius = 0.2f;

        Vector2 direction;
        direction.x = 0;
        direction.y = -1;

        float distance = 0.5f;
        LayerMask layerMask = LayerMask.GetMask("Platform");

        RaycastHit2D hitRec = Physics2D.CircleCast(origin, radius, direction, distance, layerMask);
        return hitRec.collider != null;
    }

    private void jump()
    {
        Vector2 newVelocity;
        newVelocity.x = _rigidbody.velocity.x;
        newVelocity.y = jumpSpeed;

        _rigidbody.velocity = newVelocity;

        _animator.SetBool("IsJump", true);
        jumpLeft -= 1;
        if (jumpLeft == 0)
        {
            _animator.SetTrigger("IsJumpSecond");
        } 
        else if (jumpLeft == 1)
        {
            _animator.SetTrigger("IsJumpFirst");
        }
    }

    private void climbJump()
    {
        Vector2 realClimbJumpForce;
        realClimbJumpForce.x = climbJumpForce.x * _transform.localScale.x;
        realClimbJumpForce.y = climbJumpForce.y;
        _rigidbody.AddForce(realClimbJumpForce, ForceMode2D.Impulse);

        _animator.SetTrigger("IsClimbJump");
        _animator.SetTrigger("IsJumpFirst");

        isInputEnabled = false;
        StartCoroutine(climbJumpCoroutine(climbJumpDelay));
    }

    private IEnumerator climbJumpCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        isInputEnabled = true;

        _animator.ResetTrigger("IsClimbJump");

        Vector3 newScale;
        newScale.x = -_transform.localScale.x;
        newScale.y = 1;
        newScale.z = 1;

        _transform.localScale = newScale;
    }

    private void fall()
    {
        Vector2 newVelocity;
        newVelocity.x = _rigidbody.velocity.x;
        newVelocity.y = -fallSpeed;

        _rigidbody.velocity = newVelocity;
    }

    private void sprint()
    {
        isInputEnabled = false;
        isSprintable = false;
        isSprintReset = false;

        Vector2 newVelocity;
        newVelocity.x = _transform.localScale.x * (isClimb ? sprintSpeed : -sprintSpeed);
        newVelocity.y = 0;

        _rigidbody.velocity = newVelocity;

        if (isClimb)
        {
            Vector3 newScale;
            newScale.x = -_transform.localScale.x;
            newScale.y = 1;
            newScale.z = 1;

            _transform.localScale = newScale;
        }

        _animator.SetTrigger("IsSprint");
        StartCoroutine(sprintCoroutine(sprintTime, sprintInterval));
    }

    private IEnumerator sprintCoroutine(float sprintDelay, float sprintInterval)
    {
        yield return new WaitForSeconds(sprintDelay);
        isInputEnabled = true;
        isSprintable = true;

        yield return new WaitForSeconds(sprintInterval);
        isSprintReset = true;
    }

    private void attack()
    {
        float verticalDirection = Input.GetAxis("Vertical");
        if (verticalDirection > 0)
            attackUp();
        else if (verticalDirection < 0 && !isGrounded)
            attackDown();
        else
            attackForward();
    }

    private void attackUp()
    {
        _animator.SetTrigger("IsAttackUp");
        attackUpEffect.SetActive(true);

        Vector2 detectDirection;
        detectDirection.x = 0;
        detectDirection.y = 1;

        StartCoroutine(attackCoroutine(attackUpEffect, attackEffectLifeTime, attackInterval, detectDirection, attackUpRecoil));
    }

    private void attackForward()
    {
        _animator.SetTrigger("IsAttack");
        attackForwardEffect.SetActive(true);

        Vector2 detectDirection;
        detectDirection.x = -_transform.localScale.x;
        detectDirection.y = 0;

        Vector2 recoil;
        recoil.x = _transform.localScale.x > 0 ? -attackForwardRecoil.x : attackForwardRecoil.x;
        recoil.y = attackForwardRecoil.y;

        StartCoroutine(attackCoroutine(attackForwardEffect, attackEffectLifeTime, attackInterval, detectDirection, recoil));
    }

    private void attackDown()
    {
        _animator.SetTrigger("IsAttackDown");
        attackDownEffect.SetActive(true);

        Vector2 detectDirection;
        detectDirection.x = 0;
        detectDirection.y = -1;

        StartCoroutine(attackCoroutine(attackDownEffect, attackEffectLifeTime, attackInterval, detectDirection, attackDownRecoil));
    }

    private IEnumerator attackCoroutine(GameObject attackEffect,float effectDelay, float attackInterval, Vector2 detectDirection, Vector2 attackRecoil)
    {
        Vector2 origin = _transform.position;

        float radius = 0.6f;

        float distance = 1.5f;
        LayerMask layerMask = LayerMask.GetMask("Radiance") | LayerMask.GetMask("Enemy") | LayerMask.GetMask("Trap") | LayerMask.GetMask("Switch") | LayerMask.GetMask("Projectile");

        RaycastHit2D[] hitRecList = Physics2D.CircleCastAll(origin, radius, detectDirection, distance, layerMask);

        foreach (RaycastHit2D hitRec in hitRecList)
        {
            GameObject obj = hitRec.collider.gameObject;

            string layerName = LayerMask.LayerToName(obj.layer);

            if (layerName == "Enemy")
            {
                EnemyController enemyController = obj.GetComponent<EnemyController>();
                if (enemyController != null)
                    enemyController.hurt(1);
                // Debug.Log("Player hp" + health);
            }
            else if (layerName == "Projectile")
            {
                Destroy(obj);
            }
            else if (layerName == "Radiance")
            {
                RadianceController radianceController = obj.GetComponent<RadianceController>();
                if (radianceController != null)
                    radianceController.TakeDamage(1);
            }
        }

        if (hitRecList.Length > 0)
        {
            _rigidbody.velocity = attackRecoil;
        }

        yield return new WaitForSeconds(effectDelay);

        attackEffect.SetActive(false);

        isAttackable = false;
        yield return new WaitForSeconds(attackInterval);
        isAttackable = true;
    }
}

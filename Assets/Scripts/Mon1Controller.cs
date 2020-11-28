using UnityEngine;

public class Mon1Controller : MonoBehaviour, IEnemyBehavior
{
    #region Editable fields

    [Header("Gameplay")]
    [SerializeField]
    int StartingHealth = 5;
    [SerializeField]
    float AttackReloadTime = 1.5f;
    [SerializeField]
    float Speed = 3;
    [SerializeField]
    float IdleWanderTime = 2f;
    [SerializeField]
    float ReactionTime = .2f;

    #endregion

    #region Read-only fields

    [Header("Debug")]
    [SerializeField, ReadOnly]
    int health = 1;
    [SerializeField, ReadOnly]
    int idleWanderState = 0;
    [SerializeField, ReadOnly]
    bool hasHitPlayer = false;
    [SerializeField, ReadOnly]
    bool facingRight = true;
    [SerializeField, ReadOnly]
    float attackTimer = 0;
    [SerializeField, ReadOnly]
    float idleWanderTimer = 0;
    [SerializeField, ReadOnly]
    float reactionTimer = .2f;
    [SerializeField, ReadOnly]
    float distanceFromCamera = 0;
    [SerializeField, ReadOnly]
    Vector3 velocity = Vector3.zero;

    #endregion

    #region Inaccessible fields

    const int NoState = 0;
    const int IdleState = 1;
    const int WanderState = 2;

    Transform Camera;
    Animator anim;
    Rigidbody2D body;

    #endregion

    #region IEnemyBehavior methods

    public void SetCameraReference(Transform _camera)
    {
        Camera = _camera;
        distanceFromCamera = Vector2.Distance(transform.position, Camera.position);
    }

    public void SetFaceRight(bool _facingRight)
    {
        facingRight = _facingRight;
        Vector3 newScale = transform.localScale;
        newScale.x = (facingRight) ? 1 : -1;
        transform.localScale = newScale;
    }

    public void DealDamageToEnemy(int _damage, Vector3 _damageSourceLocation, Vector2 _force)
    {
        health -= _damage;
        if(_damage > 0)
            anim.SetTrigger("Hurt");
        if (health <= 0)
        {
            anim.SetTrigger("Kill");
            anim.SetBool("Dying", true);
        }

        if (facingRight && _damageSourceLocation.x < transform.position.x)
            Flip();
        if (!facingRight && _damageSourceLocation.x > transform.position.x)
            Flip();
        if (_force != Vector2.zero)
        {
            body.velocity = Vector3.zero;
            if (facingRight)
                _force.x = -_force.x;
            body.AddForce(_force);
        }
    }

    public bool CanJumpOn(Vector3 _playerPosition)
    {
        Vector3 offset = GetComponent<CircleCollider2D>().offset;
        offset += transform.position;
        if (_playerPosition.y > offset.y)
            return true;
        return false;
    }

    public bool IsVulnerable()
    {
        return !IsAttacking();
    }

    public void Clash(Vector3 _clashPosition)
    {
        float force = (facingRight) ? -1000 : 1000;
        body.velocity = Vector3.zero;
        body.AddForce(new Vector2(force, 100));
    }

    public void Kill()
    {
        gameObject.SetActive(false);
        body.gravityScale = 1;
    }

    public int RemainingHealth() { return health; }

    #endregion

    void OnEnable()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        health = StartingHealth;
        anim.Play("Base Layer.idle");
        anim.SetBool("Dying", false);
        hasHitPlayer = false;
        idleWanderState = NoState;
    }

    void FixedUpdate()
    {
        velocity = body.velocity;
        if (CanMove())
        {
            hasHitPlayer = false;
            if(idleWanderState == NoState)
            {
                idleWanderState = (Random.Range(0, 1f) < .5f) ? IdleState : WanderState;
                bool flip = Random.Range(0, 1f) < .5f;
                if (flip)
                    Flip();
                idleWanderTimer = IdleWanderTime;
            }
            if (idleWanderTimer > 0)
                idleWanderTimer -= Time.deltaTime;
            if (idleWanderTimer <= 0)
                idleWanderState = NoState;
            if (idleWanderState == WanderState)
            {
                if (attackTimer <= 0)
                {
                    velocity.x = (facingRight) ? Speed : -Speed;
                }
                else
                {
                    velocity.x = 0;
                    attackTimer -= Time.deltaTime;
                }
            }
            else
                velocity.x = 0;

            distanceFromCamera = Vector2.Distance(transform.position, Camera.position);
            if (distanceFromCamera >= 18f)
                gameObject.SetActive(false);
        }
        else if(!IsJumping())
            velocity.x = 0;

        if (anim.isInitialized)
        {
            if (anim.GetBool("Dying"))
                velocity = Vector3.zero;

            anim.SetFloat("HorizontalSpeed", Mathf.Abs(velocity.x));
        }
        body.velocity = velocity;

    }

    public void SetIsGrounded(bool _isGrounded)
    {
        anim.SetBool("IsGrounded", _isGrounded);
    }

    public void Flip()
    {
        facingRight = !facingRight;
        Vector3 newScale = transform.localScale;
        newScale.x = -newScale.x;
        transform.localScale = newScale;
    }

    bool CanMove()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.move") || anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.idle");
    }

    public bool IsAttacking()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.attack") || anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.jump");
    }

    bool IsAttack()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.attack");
    }

    public bool IsJumping()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.jump");
    }

    public void ApplyJumpForce()
    {
        body.AddForce(new Vector2(facingRight ? 12000 : -12000, 8000));
    }

    public void TriggerJump()
    {
        velocity = body.velocity;
        velocity.x = 0;
        body.velocity = velocity;
        anim.SetTrigger("Jump");
    }

    void OnCollisionEnter2D(Collision2D _collision)
    {
        if(_collision.gameObject.tag == "Player")
        {
            PlayerController pc = _collision.gameObject.GetComponent<PlayerController>();
            if (IsJumping())
            {
                if (CanJumpOn(pc.transform.position) || pc.IsAttacking())
                    Clash(_collision.contacts[0].point);
                else
                    pc.DealDamageToPlayer(4, GetComponent<CircleCollider2D>().offset);
            }
        }
    }

    void OnCollisionStay2D(Collision2D _collision)
    {
        if(_collision.gameObject.tag == "Player")
        {
            PlayerController pc = _collision.gameObject.GetComponent<PlayerController>();
            if(IsAttack() && !hasHitPlayer)
            {
                if (pc.IsAttacking())
                    Clash(_collision.contacts[0].point);
                else
                {
                    hasHitPlayer = true;
                    pc.DealDamageToPlayer(1, GetComponent<CircleCollider2D>().offset);
                }
            }
            if(IsVulnerable())
            {
                if(reactionTimer > 0)
                    reactionTimer -= Time.deltaTime;
                if(reactionTimer <= 0 && attackTimer <= 0)
                {
                    if ((pc.transform.position.x > transform.position.x && facingRight) || (pc.transform.position.x < transform.position.x && !facingRight))
                    {
                        anim.SetTrigger("Attack");
                        attackTimer = AttackReloadTime;
                    }
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D _collision)
    {
        if(_collision.gameObject.tag == "Player")
                reactionTimer = ReactionTime;

    }
}

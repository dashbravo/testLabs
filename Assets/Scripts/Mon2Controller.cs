using UnityEngine;

public class Mon2Controller : MonoBehaviour, IEnemyBehavior {

    #region Editable fields

    [Header("Gameplay")]
    [SerializeField]
    int StartingHealth = 10;
    [SerializeField]
    int DamageUntilTrackThreshold = 3;
    [SerializeField]
    int DamageTakenSinceTracking = 0;
    [SerializeField]
    float Speed = 3;
    [SerializeField]
    float IdleWanderTime = 2f;
    [SerializeField]
    float FlyTime = 2.5f;

    #endregion

    #region Read-only fields

    [Header("Debug")]
    [SerializeField, ReadOnly]
    int health = 1;
    [SerializeField, ReadOnly]
    bool facingRight = true;
    [SerializeField, ReadOnly]
    int behaviorState = 0;
    [SerializeField, ReadOnly]
    float flyTimer = 0;
    [SerializeField, ReadOnly]
    float idleWanderTimer = 0;
    [SerializeField, ReadOnly]
    float distanceFromCamera = 0;
    [SerializeField, ReadOnly]
    Vector3 velocity = Vector3.zero;

    #endregion

    #region Inaccessible fields

    const int NoState = 0;
    const int IdleState = 1;
    const int WanderState = 2;
    const int TrackState = 3;

    Transform Camera;
    Animator anim;
    Rigidbody2D body;
    Collider2D[] colliderCheck;

    #endregion

    void Start()
    {
        colliderCheck = new Collider2D[1];
    }

    void OnEnable()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        health = StartingHealth;
        anim.Play("Base Layer.idle");
        anim.SetBool("Dying", false);
        DamageTakenSinceTracking = 0;
    }

    void FixedUpdate()
    {
        velocity = body.velocity;
        if (flyTimer > 0)
        {
            flyTimer -= Time.deltaTime;
            if (flyTimer <= 0)
                TurnFlyOff();
            int found = Physics2D.OverlapCircleNonAlloc(transform.position, 18, colliderCheck, LayerMask.GetMask("Player"));
            if (found != 0 && colliderCheck[0].gameObject.tag == "Player")
            {
                GameObject player = colliderCheck[0].gameObject;
                if (player.transform.position.x < transform.position.x && facingRight)
                    Flip();
                else if (player.transform.position.x > transform.position.x && !facingRight)
                    Flip();
                velocity.x = (facingRight) ? 3 * Speed : -3 * Speed;
            }
            else
                velocity.x = 0;
        }
        else
        {
            distanceFromCamera = Vector2.Distance(transform.position, Camera.position);
            if (distanceFromCamera >= 18f)
                gameObject.SetActive(false);
            else if (distanceFromCamera <= 6 && CanMove())
            {
                behaviorState = TrackState;
                int found = Physics2D.OverlapCircleNonAlloc(transform.position, 10, colliderCheck, LayerMask.GetMask("Player"));

                if (found != 0 && colliderCheck[0].gameObject.tag == "Player")
                {
                    GameObject player = colliderCheck[0].gameObject;
                    if (player.transform.position.x < transform.position.x && facingRight)
                        Flip();
                    else if (player.transform.position.x > transform.position.x && !facingRight)
                        Flip();
                    velocity.x = (facingRight) ? Speed : -Speed;
                }
                else
                {
                    if (Camera.transform.position.x < transform.position.x && facingRight)
                        Flip();
                    else if (Camera.transform.position.x > transform.position.x && !facingRight)
                        Flip();
                    velocity.x = 0;
                }

            }
            else if (CanMove())
            {
                if (behaviorState == NoState)
                {
                    behaviorState = (Random.Range(0, 1f) < .5f) ? IdleState : WanderState;
                    bool flip = Random.Range(0, 1f) < .5f;
                    if (flip)
                        Flip();
                    idleWanderTimer = IdleWanderTime;
                }
                if (idleWanderTimer > 0)
                    idleWanderTimer -= Time.deltaTime;
                if (idleWanderTimer <= 0)
                    behaviorState = NoState;
                if (behaviorState == WanderState)
                    velocity.x = (facingRight) ? Speed : -Speed;
                else
                    velocity.x = 0;
            }
        }
        if(anim.isInitialized)
            anim.SetFloat("HorizontalSpeed", Mathf.Abs(velocity.x));
        body.velocity = velocity;
    }

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
        if (!IsFlying())
        {
            health -= _damage;
            DamageTakenSinceTracking += _damage;
            if(_damage > 0)
                anim.SetTrigger("Hurt");
            if (health <= 0)
                anim.SetBool("Dying", true);
            else if (DamageTakenSinceTracking >= DamageUntilTrackThreshold)
            {
                DamageTakenSinceTracking = 0;
                anim.SetBool("Fly", true);
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
    }

    public bool CanJumpOn(Vector3 _playerPosition)
    {
        if (IsFlying())
            return false;
        Vector3 offset = GetComponent<CircleCollider2D>().offset;
        offset += transform.position;
        if (_playerPosition.y > offset.y)
            return true;
        return false;
    }

    public bool IsVulnerable()
    {
        return false;// !IsAttacking();
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

    bool CanMove()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.move") || anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.idle");
    }

    bool IsFlying()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.startfly") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.fly") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.endfly");
    }

    public void Flip()
    {
        facingRight = !facingRight;
        Vector3 newScale = transform.localScale;
        newScale.x = -newScale.x;
        transform.localScale = newScale;
    }

    void HarmPlayerOn()
    {
        anim.SetBool("HarmPlayer", true);
    }

    void HarmPlayerOff()
    {
        anim.SetBool("HarmPlayer", false);
    }

    void TurnFlyOff()
    {
        anim.SetBool("Fly", false);
    }

    void StartFlying()
    {
        behaviorState = NoState;
        flyTimer = FlyTime;
        HarmPlayerOn();
    }

    public bool IsIdle()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.idle");
    }

    public bool CanHarmPlayer()
    {
        return anim.GetBool("HarmPlayer");
    }

}

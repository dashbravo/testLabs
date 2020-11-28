using UnityEngine;

public class Mon4Controller : MonoBehaviour, IEnemyBehavior
{
    #region Editable fields

    [Header("Gameplay")]
    [SerializeField]
    int StartingHealth = 4;
    [SerializeField]
    float AttackReloadTime = 2f;
    [SerializeField]
    Transform ProjectileSpawnPosition;
    [SerializeField]
    GameObject Projectile;

    #endregion

    #region Read-only fields

    [Header("Debug")]
    [SerializeField, ReadOnly]
    int health = 1;
    [SerializeField, ReadOnly]
    bool facingRight = false;
    [SerializeField, ReadOnly]
    float attackTimer = 0;
    [SerializeField, ReadOnly]
    float distanceFromCamera = 0;

    #endregion

    #region Inaccessible fields

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
    }

    void FixedUpdate()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
        distanceFromCamera = Vector2.Distance(transform.position, Camera.position);
        if (distanceFromCamera >= 18f)
            gameObject.SetActive(false);
        else if (distanceFromCamera <= 10 && IsIdle())
        {
            int found = Physics2D.OverlapCircleNonAlloc(transform.position, 10, colliderCheck, LayerMask.GetMask("Player"));

            if (found != 0 && colliderCheck[0].gameObject.tag == "Player")
            {
                GameObject player = colliderCheck[0].gameObject;
                if (player.transform.position.x < transform.position.x && facingRight)
                    Flip();
                else if (player.transform.position.x > transform.position.x && !facingRight)
                    Flip();
            }
            else
            {
                if (Camera.transform.position.x < transform.position.x && facingRight)
                    Flip();
                else if (Camera.transform.position.x > transform.position.x && !facingRight)
                    Flip();
            }
            if (attackTimer <= 0)
            {
                anim.SetTrigger("Shoot");
                attackTimer = AttackReloadTime;
            }
        }
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
        newScale.x = (facingRight) ? -1 : 1;
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

    public bool IsIdle()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.idle");
    }

    public bool CanHarmPlayer()
    {
        return anim.GetBool("HarmPlayer");
    }

    void SpawnProjectile()
    {
        GameObject shot = (GameObject)Instantiate(Projectile, ProjectileSpawnPosition.position, Quaternion.identity);
        if (facingRight)
            shot.GetComponent<ProjectileController>().Flip();
        shot.GetComponent<ProjectileController>().Init();
        shot.GetComponent<ProjectileController>().InitAnimation();
    }

}

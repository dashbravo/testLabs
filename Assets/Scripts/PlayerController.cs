using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{

    #region Editable fields

    [Header("Gameplay")]
    [SerializeField]
    GameObject smallProjectile;
    [SerializeField]
    GameObject chargeProjectile;
    [SerializeField]
    GameObject superProjectile;
    [SerializeField]
    Color[] chargeColors;
    [SerializeField]
    int StartingHealth = 50;

    [Header("Physics")]
    [SerializeField]
    Transform groundCheck;
    [SerializeField]
    Transform projectileSpawnPosition;
    [SerializeField]
    Transform dashEffectSpawnPosition;
    [SerializeField]
    LayerMask whatIsGround;
    [SerializeField]
    float maxSpeed = 5f;
    [SerializeField]
    float jumpForce = 800f;
    [SerializeField]
    float DashLimitTime = .3f;
    [SerializeField]
    float UppercutLimitTime = .3f;

    [Header("Audio")]
    [SerializeField]
    AudioClip jumpClip;
    [SerializeField]
    AudioClip landClip;
    [SerializeField]
    AudioClip spinClip;
    [SerializeField]
    AudioClip dashClip;
    [SerializeField]
    AudioClip shootClip;
    [SerializeField]
    AudioClip chargeClip;
    [SerializeField]
    AudioClip chargeShootClip;
    [SerializeField]
    AudioClip superShootClip;
    [SerializeField]
    AudioClip[] hitClips = { null, null, null };

    #endregion

    #region Read-only fields
    [Header("Debug")]
    [SerializeField, ReadOnly]
    bool facingRight = true;
    [SerializeField, ReadOnly]
    bool isGrounded = false;
    [SerializeField, ReadOnly]
    bool startCharge = false;
    [SerializeField, ReadOnly]
    bool charged = false;
    [SerializeField, ReadOnly]
    int health = 1;
    [SerializeField, ReadOnly]
    int superMeter = 0;
    [SerializeField, ReadOnly]
    float chargeTimer = 0;
    [SerializeField, ReadOnly]
    float dashTime;
    [SerializeField, ReadOnly]
    float uppercutTime;
    [SerializeField, ReadOnly]
    float xAxis = 0;
    [SerializeField, ReadOnly]
    bool triggerDash = false;
    [SerializeField, ReadOnly]
    Vector2 velocity = Vector2.zero;


    #endregion

    #region Inaccessible fields

    bool continueAttack = false;
    bool jumpedOnEnemy = false;
    float groundRadius = .2f;
    const float DeadZone = .3f;
    const float HeadOffset = 1f;
    Animator anim;
    AudioSource audioSource;
    Collider2D[] colliderCheck;
    HashSet<GameObject> uppercutEnemies;

    #endregion

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        colliderCheck = new Collider2D[10];
        uppercutEnemies = new HashSet<GameObject>();
        health = StartingHealth;
    }

    // Update is called once per frame (Use this for input and state/animation changes)
    void Update()
    {
        // Is dog on the ground?
        //bool prevGrounded = isGrounded;
        isGrounded = (Physics2D.OverlapCircleNonAlloc(groundCheck.position, groundRadius, colliderCheck, whatIsGround) > 0);
        anim.SetBool("isGrounded", isGrounded);

        xAxis = Input.GetAxis("Horizontal");
        triggerDash = Input.GetButtonDown("Dash");

        velocity = GetComponent<Rigidbody2D>().velocity;
        if (!IsHurt())
            velocity.x = xAxis * maxSpeed;

        if (!IsAttacking() && !IsHurt() && !continueAttack)
        {
            if (velocity.x > 0 && !facingRight)
                Flip();
            else if (velocity.x < 0 && facingRight)
                Flip();
            GetComponent<Rigidbody2D>().velocity = velocity;
        }
        //else if (IsDashKicking())
        //    GetComponent<Rigidbody2D>().velocity = new Vector2(maxSpeed * 4, 1);

        anim.SetFloat("Horizontal", Mathf.Abs(velocity.x));
        anim.SetFloat("YVelocity", velocity.y);
        float YAxis = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                if (YAxis < -DeadZone)
                {
                    // player is attempting to jump through a platform
                    // make sure there is a platform below first
                    Vector3 offset = new Vector3(0, -.2f, 0);
                    Vector3 checkPlatform = groundCheck.position + offset;
                    Collider2D[] colliders = { null, null, null, null, null };
                    if (Physics2D.OverlapPointNonAlloc(checkPlatform, colliders, whatIsGround) > 0)
                    {
                        // no platform do regular jump
                        anim.SetTrigger("Jump");
                    }
                    else
                    {
                        // platform exists
                        transform.position = transform.position + new Vector3(0, -.75f, 0);
                        anim.SetBool("isGrounded", false);
                    }
                }
                else
                {
                    anim.SetTrigger("Jump");
                }
            }
            else if (IsFalling() && !anim.GetBool("HasSpun"))
            {
                anim.SetTrigger("Jump");
                if (velocity.y < 5f)
                    ApplySpinForce();
            }
        }

        if (isGrounded && continueAttack && !IsPunching())
        {
            continueAttack = false;
            // pick a random melee attack for the player to perform
            RandomAttack();
        }
        if (Input.GetButtonDown("Attack"))
        {
            if (YAxis > DeadZone && !anim.GetBool("HasUppercut") && dashTime == 0 && CanUppercut())
            {
                // player is pressing up so uppercut!
                anim.SetTrigger("Uppercut");
                ApplyUppercutForce();
            }
            else if (xAxis > DeadZone || xAxis < -DeadZone)
            {
                triggerDash = true;
            }
            else if (isGrounded)
            {
                if (!IsPunching())
                {
                    // pick a random melee attack for the player to perform
                    velocity = GetComponent<Rigidbody2D>().velocity;
                    velocity.x = 0;
                    GetComponent<Rigidbody2D>().velocity = velocity;
                    RandomAttack();
                }
                else
                    continueAttack = true;
            }
        }
        if (Input.GetButtonDown("Cheat"))
        {
            superMeter = 100;
        }

        if (Input.GetButtonDown("Shoot") && (IsIdle() || IsFalling()))
        {
            if (superMeter == 100)
            {
                audioSource.PlayOneShot(chargeClip);
                anim.SetBool("SuperShoot", true);
                //TODO: super effect
            }
            else
                anim.SetBool("Shoot", true);
        }
        if (Input.GetButton("Shoot"))
        {
            if (chargeTimer > .25f && !startCharge)
            {
                startCharge = true;
                audioSource.PlayOneShot(chargeClip);
                audioSource.PlayDelayed(1);
            }
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= 1)
                charged = true;
        }
        else if (chargeTimer > 0 && superMeter != 100)
        {
            charged = false;
            startCharge = false;
            GetComponent<SpriteRenderer>().color = Color.white;
            if (audioSource.isPlaying)
                audioSource.Stop();
            if (chargeTimer > 1)
                anim.SetBool("ChargeShoot", true);
            else
                anim.SetBool("Shoot", true);
        }

        if (charged)
        {
            if (chargeColors.Length > 0)
                GetComponent<SpriteRenderer>().color = chargeColors[Random.Range(0, chargeColors.Length)];
        }


        if (!anim.GetBool("HasDashed") && triggerDash && uppercutTime == 0)
        {
            anim.SetTrigger("Dash");
        }

        if (dashTime > 0)
            dashTime -= Time.deltaTime;
        else
        {
            GetComponent<Rigidbody2D>().gravityScale = 1;
            dashTime = 0;
        }
        anim.SetFloat("DashTime", dashTime);

        if (uppercutTime > 0)
            uppercutTime -= Time.deltaTime;
        else
        {
            GetComponent<Rigidbody2D>().gravityScale = 1;
            uppercutTime = 0;
        }
        anim.SetFloat("UppercutTime", uppercutTime);

    }

    void RandomAttack()
    {
        int number = Random.Range(1, 6);
        anim.SetInteger("Attack", number);
    }

    void SetHasSpin()
    {
        anim.SetBool("HasSpun", true);
    }

    void SetHasDashed()
    {
        anim.SetBool("HasDashed", true);
    }

    void SetHasUppercut()
    {
        anim.SetBool("HasUppercut", true);
    }

    void ResetAttackProperty()
    {
        anim.SetInteger("Attack", 0);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 newScale = transform.localScale;
        newScale.x = -newScale.x;
        transform.localScale = newScale;
    }

    void ResetAnimationProperties()
    {
        anim.SetBool("HasSpun", false);
        anim.SetBool("HasUppercut", false);
        anim.SetBool("HasDashed", false);
        if (IsLanding())
            audioSource.PlayOneShot(landClip);

    }

    public int GetSuperMeter()
    {
        return superMeter;
    }

    public bool IsAttacking()
    {
        return IsDashKicking() || IsUppercutting() || IsPunching();
    }

    public void Clash()
    {
        Vector3 velocity = GetComponent<Rigidbody2D>().velocity;
        velocity.x = 0;
        GetComponent<Rigidbody2D>().velocity = velocity;
        GetComponent<Rigidbody2D>().AddForce(new Vector2(facingRight ? -100 : 100, 10));
    }

    bool IsIdle()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.idle");
    }

    bool IsPunching()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.attack1") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.attack2") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.attack3") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.attack4") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.attack5");
    }

    bool IsDashKicking()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.dashkickstart")
            || anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.dashkick");
    }

    bool IsUppercutting()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.uppercut")
            || anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.uppercutend");
    }

    bool IsFalling()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.fall");
    }

    bool IsLanding()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.land");
    }

    bool IsHurt()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.hurt") || anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.hurtbad");
    }

    bool CanUppercut()
    {
        return IsFalling() || IsIdle() || IsLanding() || anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.walk") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.jump") || anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.spin");
    }

    bool IsSuper()
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.supershoot");
    }

    public void DealDamageToPlayer(int _Damage, Vector3 _damageSource)
    {
        if (IsDashKicking() || IsUppercutting() || IsHurt() || IsSuper())
            return;
        anim.SetInteger("Damage", _Damage);
        anim.SetTrigger("Hurt");
        ResetAnimationProperties();
        health -= _Damage;
        if(health <= 0)
        {
            // todo: death
        }
        superMeter += _Damage;
        if (superMeter > 100)
            superMeter = 100;
        if (facingRight && _damageSource.x < transform.position.x)
            Flip();
        if (!facingRight && _damageSource.x > transform.position.x)
            Flip();
        if(_Damage > 3)
        {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(facingRight ? -800 : 800, 500));
        }
    }

    void ResetDamageTaken()
    {
        anim.SetInteger("Damage", 0);
    }

    void ApplyJumpForce()
    {
        Vector2 currentVelocity = GetComponent<Rigidbody2D>().velocity;
        currentVelocity.y = 0;
        GetComponent<Rigidbody2D>().velocity = currentVelocity;
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0, jumpForce));
        anim.SetBool("isGrounded", false);
        audioSource.PlayOneShot(jumpClip);
    }

    void ApplySpinForce()
    {
        Vector2 currentVelocity = GetComponent<Rigidbody2D>().velocity;
        currentVelocity.y = 0;
        GetComponent<Rigidbody2D>().velocity = currentVelocity;
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0, jumpForce*.8f));
        audioSource.PlayOneShot(spinClip);
    }

    void ApplyDashForce()
    {
        Vector2 dashVelocity = new Vector2(facingRight ? maxSpeed * 4 : -maxSpeed * 4, 1);
        GetComponent<Rigidbody2D>().velocity = dashVelocity;
        GetComponent<Rigidbody2D>().gravityScale = 0;
        anim.SetBool("isGrounded", false);
        dashTime = DashLimitTime;
        audioSource.PlayOneShot(dashClip);
        GameObject effect = (GameObject)Instantiate(smallProjectile, dashEffectSpawnPosition.position, Quaternion.identity);
        effect.GetComponent<ProjectileController>().StartVelocity = Vector2.zero;
        Destroy(effect.GetComponent<Rigidbody2D>());
        effect.gameObject.transform.SetParent(gameObject.transform);
        effect.GetComponent<ProjectileController>().Init();
        if (!facingRight)
            effect.GetComponent<ProjectileController>().Flip();
        effect.GetComponent<ProjectileController>().Kill();
    }

    void ApplyUppercutForce()
    {
        Vector2 uppercutVelocity = new Vector2(facingRight ? 1 : -1, maxSpeed * 4);
        GetComponent<Rigidbody2D>().velocity = uppercutVelocity;
        GetComponent<Rigidbody2D>().gravityScale = 0;
        anim.SetBool("isGrounded", false);
        uppercutTime = UppercutLimitTime;
        audioSource.PlayOneShot(dashClip);
        GameObject effect = (GameObject)Instantiate(smallProjectile, groundCheck.position, Quaternion.Euler(0, 0, 90));
        effect.GetComponent<ProjectileController>().StartVelocity = Vector2.zero;
        Destroy(effect.GetComponent<Rigidbody2D>());
        effect.gameObject.transform.SetParent(gameObject.transform);
        effect.GetComponent<ProjectileController>().Init();
        effect.GetComponent<ProjectileController>().Kill();
    }


    void DetermineShot()
    {
        if (superMeter == 100 && superProjectile)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            superMeter = 0;
            ShootProjectile(superProjectile, superShootClip);
        }
        else if (chargeTimer > 1 && chargeProjectile)
            ShootProjectile(chargeProjectile, chargeShootClip);
        else
            ShootProjectile(smallProjectile, shootClip);
    }

    void ShootProjectile(GameObject _projectile, AudioClip _sound)
    {
        anim.SetBool("Shoot", false);
        anim.SetBool("ChargeShoot", false);
        anim.SetBool("SuperShoot", false);
        GameObject shot = (GameObject)Instantiate(_projectile, projectileSpawnPosition.position, Quaternion.identity);
        if (!facingRight)
            shot.GetComponent<ProjectileController>().Flip();
        shot.GetComponent<ProjectileController>().Init();
        shot.GetComponent<ProjectileController>().InitAnimation();
        audioSource.PlayOneShot(_sound);
        chargeTimer = 0;
        charged = false;
        startCharge = false;
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    void PlayRandomHitClip()
    {
        audioSource.PlayOneShot(hitClips[Random.Range(0, hitClips.Length)]);
    }

    void OnTriggerEnter2D(Collider2D _other)
    {
        if(_other.tag == "Enemy")
        {
            if (IsDashKicking())
            {
                _other.gameObject.GetComponent<IEnemyBehavior>().DealDamageToEnemy(4, transform.position, new Vector2(10000, 1000));
                superMeter += 4;
                if (superMeter > 100)
                    superMeter = 100;
            }
            else if (IsUppercutting())
            {
                if (!uppercutEnemies.Contains(_other.gameObject))
                {
                    _other.gameObject.GetComponent<IEnemyBehavior>().DealDamageToEnemy(5, transform.position, new Vector2(1000, 10000));
                    superMeter += 5;
                    if (superMeter > 100)
                        superMeter = 100;
                    uppercutEnemies.Add(_other.gameObject);
                }
            }
            else
            {
                _other.gameObject.GetComponent<IEnemyBehavior>().DealDamageToEnemy(1, transform.position, new Vector2(2000, 100));
                superMeter += 1;
                if (superMeter > 100)
                    superMeter = 100;
            }
            PlayRandomHitClip();
        }
    }

    void OnCollisionEnter2D(Collision2D _collision)
    {
        if(_collision.gameObject.tag == "Enemy")
        {
            if(!jumpedOnEnemy && _collision.gameObject.GetComponent<IEnemyBehavior>().CanJumpOn(transform.position))
            {
                _collision.gameObject.GetComponent<IEnemyBehavior>().DealDamageToEnemy(3, transform.position, Vector2.zero);
                superMeter += 3;
                if (superMeter > 100)
                    superMeter = 100;
                PlayRandomHitClip();
                ResetAnimationProperties();
                ApplyJumpForce();
                jumpedOnEnemy = true;
            }
            else if(IsUppercutting())
            {
                if (!uppercutEnemies.Contains(_collision.gameObject))
                {
                    _collision.gameObject.GetComponent<IEnemyBehavior>().DealDamageToEnemy(5, transform.position, new Vector2(1000, 10000));
                    superMeter += 5;
                    if (superMeter > 100)
                        superMeter = 100;
                    uppercutEnemies.Add(_collision.gameObject);
                }
            }
            else if(_collision.gameObject.transform.position.y >= transform.position.y + HeadOffset)
            {
                _collision.gameObject.GetComponent<IEnemyBehavior>().DealDamageToEnemy(0, transform.position, new Vector2(100, 5000));
            }
        }
    }
    void OnCollisionExit2D(Collision2D _collision)
    {
        if(_collision.gameObject.tag == "Enemy")
        {
            if(jumpedOnEnemy)
                jumpedOnEnemy = false;
            uppercutEnemies.Remove(_collision.gameObject);
        }
    }

}

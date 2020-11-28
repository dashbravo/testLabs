using UnityEngine;

public class ProjectileController : MonoBehaviour
{

    public bool PlayerOwned = true;
    public bool PassThroughEnemies = false;
    public bool PassThroughWalls = false;
    public int Damage = 1;
    public float LifeTime = 1.2f;
    public Vector2 StartVelocity = new Vector2(20, 0);

    bool startLife = false;
    Animator anim;
    Rigidbody2D body = null;

    void Update()
    {
        if (startLife)
        {
            if (LifeTime > 0)
            {
                LifeTime -= Time.deltaTime;
            }
            else
                Kill();
        }
    }

    public void Flip()
    {
        Vector3 newScale = transform.localScale;
        newScale.x = -newScale.x;
        transform.localScale = newScale;
        StartVelocity.x = -StartVelocity.x;
    }

    public void Init()
    {
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        if(body)
            body.velocity = StartVelocity;
    }
    public void InitAnimation()
    {
        anim.SetTrigger("Start");
        startLife = true;
    }

    public void Kill()
    {
        anim.SetTrigger("Kill");
        if(body)
            body.velocity = Vector3.zero;
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }

    void Remove()
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D _other)
    {
        if (startLife)
        {
            if (PlayerOwned)
            {
                if (_other.tag == "Enemy")
                {
                    _other.gameObject.GetComponent<IEnemyBehavior>().DealDamageToEnemy(Damage, transform.position, new Vector2(2000, 100));
                    if (_other.gameObject.GetComponent<IEnemyBehavior>().RemainingHealth() > Damage)
                        Kill();
                    else if (!PassThroughEnemies)
                        Kill();
                }
                else if (_other.tag == "Ground" && !PassThroughWalls)
                    Kill();
            }
            else
            {
                if (_other.tag == "Player")
                {
                    _other.GetComponent<PlayerController>().DealDamageToPlayer(Damage, transform.position);
                    Kill();
                }
            }
        }
    }

}

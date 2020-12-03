using UnityEngine;

public class Mon2FlyTrigger : MonoBehaviour
{
    bool hasHurtPlayer = false;

    Mon2Controller Enemy;

    void Start()
    {
        Enemy = transform.parent.gameObject.GetComponent<Mon2Controller>();
    }

    void OnTriggerEnter2D(Collider2D _other)
    {
        if (Enemy.CanHarmPlayer())
        {
            if (_other.tag == "Player")
            {
                if (!hasHurtPlayer)
                {
                    _other.gameObject.GetComponent<PlayerController>().DealDamageToPlayer(4, transform.position);
                    hasHurtPlayer = true;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D _other)
    {
        if (_other.tag == "Player")
            hasHurtPlayer = false;
    }
}

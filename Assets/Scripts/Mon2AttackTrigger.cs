using UnityEngine;

public class Mon2AttackTrigger : MonoBehaviour {

    #region Editable fields

    [SerializeField]
    float AttackReloadTime = 1.5f;

    #endregion

    #region Inaccessible fields

    float attackTimer = 0;
    GameObject parent;

    #endregion

    void Start()
    {
        parent = transform.parent.gameObject;
    }

    void FixedUpdate()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.tag == "Player" && attackTimer <= 0 && parent.GetComponent<Mon2Controller>().IsIdle())
        {
            parent.GetComponent<Animator>().SetTrigger("Attack");
            attackTimer = AttackReloadTime;
        }
    }

    void OnTriggerStay2D(Collider2D _other)
    {
        if (_other.tag == "Player")
        {
            if (parent.GetComponent<Mon2Controller>().CanHarmPlayer())
                _other.gameObject.GetComponent<PlayerController>().DealDamageToPlayer(3, transform.position);
            else if (attackTimer <= 0)
            {
                parent.GetComponent<Animator>().SetTrigger("Attack");
                attackTimer = AttackReloadTime;
            }
        }
    }
}

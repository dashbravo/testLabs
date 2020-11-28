using UnityEngine;

public class Mon4BiteTrigger : MonoBehaviour
{

    #region Editable fields

    [SerializeField]
    float BiteReloadTime = 1.5f;

    #endregion

    #region Inaccessible fields

    float biteTimer = 0;
    GameObject parent;

    #endregion

    void Start()
    {
        parent = transform.parent.gameObject;
    }

    void FixedUpdate()
    {
        if (biteTimer > 0)
            biteTimer -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.tag == "Player" && biteTimer <= 0 && parent.GetComponent<Mon4Controller>().IsIdle())
        {
            parent.GetComponent<Animator>().SetTrigger("Bite");
            biteTimer = BiteReloadTime;
        }
    }

    void OnTriggerStay2D(Collider2D _other)
    {
        if (_other.tag == "Player")
        {
            if(parent.GetComponent<Mon4Controller>().CanHarmPlayer())
                _other.gameObject.GetComponent<PlayerController>().DealDamageToPlayer(5, transform.position);
            else if(biteTimer <= 0)
            {
                parent.GetComponent<Animator>().SetTrigger("Bite");
                biteTimer = BiteReloadTime;
            }
        }
    }

}

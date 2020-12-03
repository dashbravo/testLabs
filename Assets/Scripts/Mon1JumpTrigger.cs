using UnityEngine;

public class Mon1JumpTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D _other)
    {
        if(_other.tag == "Player")
        {
            transform.parent.gameObject.GetComponent<Mon1Controller>().TriggerJump();
        }
    }
}
using UnityEngine;

public class Mon1GroundCheck : MonoBehaviour {

	void OnCollisionEnter2D()
    {
        transform.parent.GetComponent<Mon1Controller>().SetIsGrounded(true);
    }

    void OnCollisionExit2D()
    {
        transform.parent.GetComponent<Mon1Controller>().SetIsGrounded(false);
    }
}

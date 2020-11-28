using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    [SerializeField]
    float HighestPoint;
    [SerializeField]
    float LowestPoint;
    [SerializeField]
    float Speed;
    [SerializeField]
    bool GoingUp = true;

    Rigidbody2D rb;

	// Use this for initialization
	void Start ()
    {
        rb = GetComponent<Rigidbody2D>();	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(transform.position.y > HighestPoint && GoingUp)
            GoingUp = false;
        if (transform.position.y < LowestPoint && !GoingUp)
            GoingUp = true;

        Vector3 velocity = new Vector3(0, (GoingUp) ? Speed : -Speed, 0);
        rb.velocity = velocity;
	}
}

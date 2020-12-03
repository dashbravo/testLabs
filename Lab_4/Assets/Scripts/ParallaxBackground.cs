using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField]
    Vector2 BackgroundSize = Vector2.zero;
    [SerializeField]
    Vector2 Offset = Vector2.zero;
    [SerializeField]
    Vector2 LevelSize = new Vector2(100, 100);
    [SerializeField]
    bool UseBackgroundBounds = false;
    [SerializeField]
    Vector2 BackgroundMaxBounds = new Vector2(100, 100);
    [SerializeField]
    Vector2 BackgroundMinBounds = new Vector2(100, 100);

    Vector2 cameraBounds = Vector2.zero;
    Vector2 Minimum = Vector2.zero;
    Vector2 Maximum = Vector2.zero;
    float verticalExtents;
    float horizontalExtents;

    // Use this for initialization
    void Start()
    {
        verticalExtents = Camera.main.GetComponent<Camera>().orthographicSize;
        horizontalExtents = verticalExtents * Screen.width / Screen.height;

        Minimum.x = horizontalExtents;
        Maximum.x = LevelSize.x - horizontalExtents;
        Minimum.y = verticalExtents;
        Maximum.y = LevelSize.y - verticalExtents;
        cameraBounds.x = Maximum.x - Minimum.x;
        cameraBounds.y = Maximum.y - Minimum.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 cameraPosition = Camera.main.GetComponent<Camera>().transform.position;

        Vector2 position = Vector2.zero;
        position.x = (cameraPosition.x - Minimum.x) / cameraBounds.x;
        position.x *= LevelSize.x - BackgroundSize.x;
        position.x += BackgroundSize.x / 2f;

        position.y = (cameraPosition.y - Minimum.y) / cameraBounds.y;
        position.y *= LevelSize.y - BackgroundSize.y;
        position.y += BackgroundSize.y / 2f;

        if (UseBackgroundBounds)
        {
            if (position.x < BackgroundMinBounds.x)
                position.x = BackgroundMinBounds.x;
            if (position.x > BackgroundMaxBounds.x)
                position.x = BackgroundMaxBounds.x;
            if (position.y < BackgroundMinBounds.y)
                position.y = BackgroundMinBounds.y;
            if (position.y > BackgroundMaxBounds.y)
                position.y = BackgroundMaxBounds.y;
        }

        position += Offset;

        transform.position = position;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero; 
    Vector3 startingPosition;
    Vector3 targetPosition; 
    Vector3 currentVelocity;
    Vector3 cameraPosition; 

    // Start is called before the first frame update
    void Start()
    {
       cameraPosition = Camera.main.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //portions of the following code are from lab psuedocode (FullSail, 2020)
        float distance = Vector3.Distance(targetPosition, cameraPosition); 
        float LevelMinX = 0;
        float LevelMinY = 0;
        float LevelMaxX = 64;
        float LevelMaxY = 64;

        float HalfCameraHeight = GetComponent<Camera>().orthographicSize;
        float HalfCameraWidth = HalfCameraHeight * GetComponent<Camera>().aspect;

        float CameraMinX = LevelMinX + HalfCameraWidth;
        float CameraMaxX = LevelMaxX - HalfCameraWidth;
        float CameraMinY = LevelMinY + HalfCameraHeight;
        float CameraMaxY = LevelMaxY - HalfCameraHeight;

        //alter code from smoothdamp article link in handout
        //make target position z value match the camera!
        startingPosition = Camera.main.transform.position;
        targetPosition = target.transform.TransformPoint(0, 0, -10);

        if (distance > 2 || distance < -2) //this distance
        {
            transform.position = Vector3.SmoothDamp(startingPosition, targetPosition, ref velocity, smoothTime);
            float newX = Mathf.Clamp(transform.position.x, CameraMinX, CameraMaxX);
            float newY = Mathf.Clamp(transform.position.y, CameraMinY, CameraMaxY);
            ////apply the new x and y
            transform.position = new Vector3(newX, newY, transform.position.z);
        }


    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool rotateCamera = false;
    public Transform PlayerTransform;

    public Vector3 CameraOffset = new Vector3(0, 6.5f, 0);

    [Range(0.01f, 1.0f)] public float SmoothFactor = 0.5f;
    
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerTransform)
            CameraOffset = transform.position - PlayerTransform.position;
    }

    // Late Update is called after Update methods
    void LateUpdate()
    {
        if (PlayerTransform == null)
            return;
        
        Vector3 newPos = PlayerTransform.position + CameraOffset;

        transform.position = Vector3.Slerp(transform.position, newPos, SmoothFactor);

        if (rotateCamera)
        {
            Vector3 playerRotation = PlayerTransform.rotation.eulerAngles;

            Quaternion newCameraRotation = Quaternion.Euler(90, 0, 360 - playerRotation.y);

            transform.rotation = Quaternion.Slerp(transform.rotation, newCameraRotation, 7.5f * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
}

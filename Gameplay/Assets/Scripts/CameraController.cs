using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform PlayerTransform;

    private Vector3 CameraOffset;

    [Range(0.01f, 1.0f)] public float SmoothFactor = 0.5f;
    
    // Start is called before the first frame update
    void Start()
    {
        CameraOffset = transform.position - PlayerTransform.position;
    }

    // Late Update is called after Update methods
    void LateUpdate()
    {
        Vector3 newPos = PlayerTransform.position + CameraOffset;

        transform.position = Vector3.Slerp(transform.position, newPos, SmoothFactor);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerTransform;
    public int depth = -20;

    // follows local player
    void Update()
    {
        if (playerTransform != null)
        {
            transform.position = playerTransform.position + new Vector3(0, 0, depth);
        }
    }

    public void setTarget(Transform target)
    {
        playerTransform = target;
    }
}

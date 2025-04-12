using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3   offset;
    [SerializeField] private float     speed;
    private                  Transform target;

    private void OnEnable()
    {
        TankSpawner.OnPlayerSpawned += SetTarget;
    }

    private void OnDisable()
    {
        TankSpawner.OnPlayerSpawned -= SetTarget;
    }

    private void SetTarget(Transform playerTransform)
    {
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);
        target             = playerTransform;
        offset             = transform.position - target.position;
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.fixedDeltaTime * speed);
        }
    }
}
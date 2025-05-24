using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SetUpCinemachine : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;

    private void OnEnable()
    {
        TankSpawner.OnPlayerSpawned += SetFollowAndLookAtTarget;
    }

    private void OnDisable()
    {
        TankSpawner.OnPlayerSpawned -= SetFollowAndLookAtTarget;
    }

    private void Awake()
    {
        if (virtualCamera == null)
        {
            virtualCamera = this.GetComponent<CinemachineVirtualCamera>();
        }
    }

    public void SetFollowAndLookAtTarget(Transform playerTransform)
    {
        transform.position   = playerTransform.position;
        virtualCamera.Follow = playerTransform;
        virtualCamera.LookAt = playerTransform;
    }
}
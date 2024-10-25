using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    public GameObject cameraHolder;
    public override void OnNetworkSpawn()
    {
        cameraHolder.SetActive(IsOwner);
    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }
}

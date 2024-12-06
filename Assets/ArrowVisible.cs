using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class ArrowVisible : NetworkBehaviour
{
    // Start is called before the first frame update

    public override void OnNetworkSpawn()
    {
        gameObject.SetActive(IsOwner);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

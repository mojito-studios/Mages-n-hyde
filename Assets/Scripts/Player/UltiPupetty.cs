using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UltiPupetty : NetworkBehaviour
{
    public Player caster;
    public Transform target;
    void Start()
    {
        StartCoroutine(DestroyObject());
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position;
        //transform.rotation = target.rotation;
    }
    private IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(caster.ultiTime);
        NetworkObject.Despawn();
    }
}

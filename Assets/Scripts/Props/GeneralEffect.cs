using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class GeneralEffect : NetworkBehaviour
{
    private Animator animator;
    private bool animationEnded = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animationEnded)
        {
            animationEnded = true;
            if(IsSpawned) NetworkObject.Despawn();
            else Destroy(gameObject);
        }
    }
}


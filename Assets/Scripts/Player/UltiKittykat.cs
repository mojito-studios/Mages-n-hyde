using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UltiKittykat : NetworkBehaviour
{
    public Player caster;
    private Animator animator;
    private bool animationEnded = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animationEnded)
        {
            animationEnded = true;
            NetworkObject.Despawn();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class AnimationController : NetworkBehaviour
{
    [SerializeField] private Animator playerAnim;
    [SerializeField] public bool canFlip;
    [SerializeField] public bool canFlipInv;
    [SerializeField] bool facingRight = true;
    [SerializeField]SpriteRenderer sp;
    void Start()
    {
        playerAnim = GetComponent<Animator>();
        sp = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        sp.transform.rotation = Quaternion.identity;
    }

   public void ProcessInputs(Vector3 input)
    {

    }

    [Rpc(SendTo.Server)]
    public void AnimateMovementRpc(Vector3 input)
    {
        AnimateMovement2Rpc();
    }

    [Rpc(SendTo.Everyone)]
    public void AnimateMovement2Rpc()
    {
        Vector3 direction = GetComponentInParent<Player>().gameObject.GetComponent<Transform>().up;
        direction.Normalize();
        playerAnim.SetFloat("MoveX", direction.x);
        playerAnim.SetFloat("MoveY", direction.y);
        if (canFlip) CheckFlip(direction);
    }

    [Rpc(SendTo.Everyone)]
    public void AnimateAttackRpc()
    {
        playerAnim.SetBool("isAttacking", true);
    }

    [Rpc(SendTo.Everyone)]
    public void AnimateUltiRpc()
    {
        
        playerAnim.SetBool("isUlti", true);
    }

    [Rpc(SendTo.Everyone)]
    public void EndUltiRpc()
    {
        playerAnim.SetBool("isUlti", false);
    }

    [Rpc(SendTo.Everyone)]
    public void isWalkingRpc(bool walk)
    {
        playerAnim.SetBool("isWalking", walk);
    }

    public void Flip()
    {
        sp.GetComponent<SpriteRenderer>(); 
        sp.flipX = facingRight;

        facingRight = !facingRight;
    }

    public void CheckFlip(Vector3 direction)
    {
        if (canFlipInv == false)
        {
            if ((direction.x < 0.0000 && facingRight) || (direction.x > 0.0000 && !facingRight)) Flip();
        }
        else
        {
            if ((direction.x > 0.0000 && facingRight) || (direction.x < 0.0000 && !facingRight)) Flip();
        }
        
    }
}

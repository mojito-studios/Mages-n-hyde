using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator playerAnim;
    public bool canFlip;
    bool facingLeft;
    void Start()
    {
        playerAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public void ProcessInputs(Vector3 input)
    {

    }

    public void AnimateMovement()
    {

    }

    public void AnimateAttack()
    {

    }

    public void AnimateUlti()
    {

    }
    public void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        facingLeft = !facingLeft;
    }

    public void checkFlip(float x)
    {
        if(x < 0 && !facingLeft || x> 0 && facingLeft)
            Flip();
    }
}

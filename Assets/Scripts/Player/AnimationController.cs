using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator playerAnim;
    public bool canFlip;
    bool facingRight = true;
    SpriteRenderer sp;
    void Start()
    {
        playerAnim = GetComponent<Animator>();
        sp = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public void ProcessInputs(Vector3 input)
    {

    }

    public void AnimateMovement(Vector3 input)
    {
        input.Normalize(); //No va bien del todo pero es lo mejor que pude hacer a a las 5 am
        playerAnim.SetFloat("MoveX", input.x);
        playerAnim.SetFloat("MoveY", input.y);
        playerAnim.SetFloat("MoveMagnitude", input.magnitude);


    }

    public void AnimateAttack()
    {
        //anim.SetAttack a verdadero cuando pulse el botón de ataque. Mirar si afecta de la misma manera que cuando está hacindo cualquier acción y ataca

    }

    public void AnimateUlti()
    {
        //anim.SetUlti a verdadero cuando pulse el botón de Ulti.
    }
    public void Flip()
    {
        sp.GetComponent<SpriteRenderer>(); 
        sp.flipX = facingRight;

        facingRight = !facingRight;
    }

    public void CheckFlip(float x)
    {
        if(x < 0 && facingRight|| x> 0 && !facingRight )
            Flip();
    }
}

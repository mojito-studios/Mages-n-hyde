using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UltiGrimm : NetworkBehaviour
{
    public Player caster;
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && caster.teamAssign != collision.gameObject.GetComponent<Player>().teamAssign) //&& caster.teamAssign != collision.gameObject.GetComponent<Player>().teamAssign
        {
            if (!collision.gameObject.GetComponent<Player>().inmune.Value)
            {
                Player player = collision.GetComponent<Player>();
                collision.GetComponent<Player>().canMove = false;
                StartCoroutine(HitPlayer(player));
            }
        }


    }

    private void Start()
    {
        StartCoroutine(DestroyObject());
    }

    private IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(caster.ultiTime);
        NetworkObject.Despawn();
    }
    private IEnumerator HitPlayer(Player player)
    {
        float time = 0;
        while (time < caster.ultiTime)
        {
            if (!(player.health.Value - caster.ultidamage * 10 > 0)) { caster.kill(); player.die(caster); }
            else { player.assistantAssign(caster); }
            player.GetComponent<Player>().getHit(caster.ultidamage);
            time += 1;
        }
        yield return new WaitForSeconds(1f);
        player.canMove = true;
        NetworkObject.Despawn();
    }
}

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
        if (collision.gameObject == caster) return;

        if (collision.gameObject.tag == "Player" && caster.teamAssign != collision.gameObject.GetComponentInParent<Player>().teamAssign) //&& caster.teamAssign != collision.gameObject.GetComponent<Player>().teamAssign
        {
            if (!collision.gameObject.GetComponentInParent<Player>().inmune.Value)
            {
                Player player = collision.GetComponentInParent<Player>();
                collision.GetComponentInParent<Player>().canMove = false;
                StartCoroutine(HitPlayer(player));
            }
        }

        if ((collision.gameObject.tag == "Team2Tower" && caster.teamTower.tag == "Team1Tower") | (collision.gameObject.tag == "Team1Tower" && caster.teamTower.tag == "Team2Tower"))
        {
            Tower tower = collision.gameObject.GetComponent<Tower>();
            StartCoroutine(HitTower(tower));
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
            player.GetComponentInParent<Player>().getHit(caster.ultidamage);
            yield return new WaitForSeconds(1f);
            time += 1;
        }
        yield return new WaitForSeconds(1f);
        player.canMove = true;
    }

    private IEnumerator HitTower(Tower t)
    {
        float time = 0;
        while (time < caster.ultiTime)
        {
            if (t.GetIsDefending()) t.DamageShields(caster.ultidamage * 10);
            else t.DamageTower(caster.ultidamage);
            yield return new WaitForSeconds(1f);
            time += 1;
        }
        yield return new WaitForSeconds(1f);
    }
}

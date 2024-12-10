using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveSpell : NetworkBehaviour
{
    [SerializeField] private float spellForce;
    public Player caster;

    // Update is called once per frame
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GetComponent<Rigidbody2D>().velocity = this.transform.up * spellForce;
        RangeServerRpc(caster.range);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == caster) return;
        if (collision.gameObject.tag == "Player" && caster.teamAssign != collision.gameObject.GetComponentInParent<Player>().teamAssign) //&& caster.teamAssign != collision.gameObject.GetComponent<Player>().teamAssign
        {
            if (!collision.gameObject.GetComponentInParent<Player>().inmune.Value)
            {
                Player player = collision.gameObject.GetComponentInParent<Player>();
                if (!(player.health.Value - caster.attack * 10 > 0)) { caster.kill(); player.die(caster); }
                else { player.assistantAssign(caster); }
                player.getHit(caster.attack);
            }
        }
        if((collision.gameObject.tag == "Team2Tower" & caster.teamTower.tag == "Team1Tower") | (collision.gameObject.tag == "Team1Tower" & caster.teamTower.tag == "Team2Tower"))
        //if(collision.gameObject.tag == "Team2Tower" || collision.gameObject.tag == "Team1Tower")
        {
            Tower torre = collision.gameObject.GetComponent<Tower>();
            if(torre.GetIsDefending()) torre.DamageShields(caster.attack*10);
            else torre.DamageTower(caster.attack);
        }
        DestroyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc()
    {
        Destroy(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RangeServerRpc(float range)
    {
        Destroy(gameObject, range);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveUlti : NetworkBehaviour
{
    [SerializeField] private float spellForce;
    public Player caster;
    public float damage;

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
        if (collision.gameObject.tag == "Player" && caster.teamAssign != collision.gameObject.GetComponent<Player>().teamAssign) //&& caster.teamAssign != collision.gameObject.GetComponent<Player>().teamAssign
        {
            if (!collision.gameObject.GetComponent<Player>().inmune.Value)
            {
                Player player = collision.gameObject.GetComponent<Player>();
                if (!(player.health.Value - damage * 10 > 0)) { caster.kill(); player.die(caster); }
                else { player.assistantAssign(caster); }
                player.getHit(damage);
            }
        }
        if((collision.gameObject.tag == "Team2Tower" & caster.teamTower.tag == "Team1Tower") | (collision.gameObject.tag == "Team1Tower" & caster.teamTower.tag == "Team2Tower"))
        //if(collision.gameObject.tag == "Team2Tower" || collision.gameObject.tag == "Team1Tower")
        {
            Tower torre = collision.gameObject.GetComponent<Tower>();
            if(torre.GetIsDefending()) torre.DamageShields();
            else torre.DamageTower(damage);
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

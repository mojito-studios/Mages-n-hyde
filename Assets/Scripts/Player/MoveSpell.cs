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
        Destroy(this.gameObject, 1);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == caster) return;
        if (collision.gameObject.tag == "Player" && caster.teamAssign != collision.gameObject.GetComponent<Player>().teamAssign) //&& caster.teamAssign != collision.gameObject.GetComponent<Player>().teamAssign
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player.health.Value-caster.attack*10 !> 0) caster.kill();
            player.getHit(caster.attack);
        }
        if((collision.gameObject.tag == "Team2Tower" & caster.teamAssign == 0) | (collision.gameObject.tag == "Team1Tower" & caster.teamAssign == 1))
        //if(collision.gameObject.tag == "Team2Tower" || collision.gameObject.tag == "Team1Tower")
        {

            Tower torre = collision.gameObject.GetComponent<Tower>();
            if(torre.GetIsDefending()) torre.DamageShields();
            else torre.DamageTower(caster.attack);
        }
        DestroyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc()
    {
        Destroy(gameObject);
    }
}

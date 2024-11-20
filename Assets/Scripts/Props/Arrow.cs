using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;

public class Arrow : NetworkBehaviour
{
    private int _damage;
    private int _arrowForce = 2;
    public Tower casterTower;
    public Player caster;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
       GetComponent<Rigidbody2D>().velocity = this.transform.up * _arrowForce;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "PowerUp") return;
        if (collision.gameObject.tag == "Player") 
        {
            ulong tId = collision.gameObject.GetComponent<Player>().GetTeamTower();
            Tower playerTower = NetworkManager.Singleton.SpawnManager.SpawnedObjects[tId].GetComponent<Tower>();
            if (casterTower.tag != playerTower.tag)
            {
                Player hitPlayer = collision.gameObject.GetComponent<Player>();
                if (!(hitPlayer.health.Value - _damage * 10 > 0)) { caster.kill(); hitPlayer.die(caster); }
                hitPlayer.assistantAssign(caster);
                collision.gameObject.GetComponent<Player>().getHit(_damage);
            }
           
           
          
        }
       
        if (collision.gameObject.tag == "Team2Tower" || collision.gameObject.tag == "Team1Tower")
        {

            Tower colisionTower = collision.gameObject.GetComponent<Tower>();
            if (casterTower.tag != colisionTower.tag)
            {
                if (colisionTower.GetIsDefending()) colisionTower.DamageShields();
                else colisionTower.DamageTower(_damage);
            }
            

        }
        DestroyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc()
    {
        Destroy(gameObject);
    }
}



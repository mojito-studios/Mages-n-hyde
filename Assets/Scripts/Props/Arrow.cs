using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine.Tilemaps;

public class Arrow : NetworkBehaviour
{
    [SerializeField]private int _damage = 5;
    private int _arrowForce = 7;
    public Tower casterTower;
    public Player caster;
    public float bounds;
 
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
       GetComponent<Rigidbody2D>().velocity = -this.transform.up * _arrowForce;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "PowerUp") return;
        if (collision.gameObject.tag == "Player") 
        {
            if (!collision.gameObject.GetComponent<Player>().inmune.Value)
            {
                ulong tId = collision.gameObject.GetComponent<Player>().GetTeamTower();
                Tower playerTower = NetworkManager.Singleton.SpawnManager.SpawnedObjects[tId].GetComponent<Tower>();
                if (casterTower.tag != playerTower.tag)
                {
                    Player hitPlayer = collision.gameObject.GetComponent<Player>();
                    if (!(hitPlayer.health.Value - _damage * 10 > 0)) { caster.kill(); hitPlayer.die(caster); }
                    else { hitPlayer.assistantAssign(caster); }
                    collision.gameObject.GetComponent<Player>().getHit(_damage);
                    NetworkObject.Despawn();
                }
            }
           
          
        }
       
        if (collision.gameObject.tag == "Team2Tower" || collision.gameObject.tag == "Team1Tower")
        {

            Tower colisionTower = collision.gameObject.GetComponent<Tower>();
            if (casterTower.tag != colisionTower.tag)
            {
                if (colisionTower.GetIsDefending()) colisionTower.DamageShields(_damage);
                else colisionTower.DamageTower(_damage);
                NetworkObject.Despawn();
            }
            

        }
    }

    private void Update()
    {
        if (transform.position.y < bounds) NetworkObject.Despawn();
    }

   
   
}



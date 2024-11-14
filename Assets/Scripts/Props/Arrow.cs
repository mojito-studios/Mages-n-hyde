using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Arrow : NetworkBehaviour
{
    private int _damage;
    private int _arrowForce = 2;
    public Tower casterTower;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
       GetComponent<Rigidbody2D>().velocity = this.transform.up * _arrowForce;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player") 
        {
            ulong tId = collision.gameObject.GetComponent<Player>().GetTeamTower();
            Tower playerTower = NetworkManager.Singleton.SpawnManager.SpawnedObjects[tId].GetComponent<Tower>();
            if (casterTower.tag != playerTower.tag)
            {
                collision.gameObject.GetComponent<Player>().getHit();
            }
           
           
          
        }
       
        if (collision.gameObject.tag == "Team2Tower" || collision.gameObject.tag == "Team1Tower")
        {

            Tower colisionToweer = collision.gameObject.GetComponent<Tower>();
            if (casterTower.tag != colisionToweer.tag)
            {
                if (colisionToweer.GetIsDefending()) colisionToweer.DamageShields();
                else colisionToweer.DamageTower();
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



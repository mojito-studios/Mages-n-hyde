using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveSpell : NetworkBehaviour
{
    [SerializeField] private float spellForce;
    private Rigidbody2D rb;
    public Player caster;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GetComponent<Rigidbody2D>().velocity = this.transform.up * spellForce;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player") //&& caster.teamAssign != collision.gameObject.GetComponent<Player>().teamAssign
        {
            Debug.Log("Player");
            collision.gameObject.GetComponent<Player>().getHit();
        }
        //if((collision.gameObject.tag == "Team2Tower" & caster.teamAssign == "Team1") | (collision.gameObject.tag == "Team1Tower" & caster.teamAssign == "Team2"))
        if(collision.gameObject.tag == "Team2Tower" || collision.gameObject.tag == "Team1Tower")
        {

            Tower torre = collision.gameObject.GetComponent<Tower>();
            if(torre.GetIsDefending()) torre.DamageShields();
            else torre.DamageTower();
        }
        DestroyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc()
    {
        Destroy(gameObject);
    }
}

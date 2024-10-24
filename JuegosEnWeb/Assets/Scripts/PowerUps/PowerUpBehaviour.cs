using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerUpBehaviour : NetworkBehaviour
{
    //1 Minions 2 Torretas 3 Escudo de torre 4 curar torre
    private int _puType;
    private Player _player;
   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || !NetworkManager.Singleton.IsServer) return; //Si interacciona con cualquier otro objeto con colliders nos da igual, y si no es el server entonces no tiene validez para evitar las trampas
        _player = collision.GetComponent<Player>();
        ExecutePowerUp();
        NetworkObject.Despawn();
        GameManager.Instance.puInScene--;

    }
    void Start()
    {
        _puType = Random.Range(1, 5);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ExecutePowerUp() //Función que según el powerUp hace una cosa u otra;
    {
        switch (_puType)
        {
            default:
                break;
            case 1:
                SpawnMinions();
                break;
            case 2:
                CreateTurrets();
                break;
            case 3:
                RiseTowerDefenses();
                break;
            case 4:
                HealTower();
                break;

        }
    }

    public void SpawnMinions()
    {
        Debug.Log("Minions");
    }

    public void CreateTurrets()
    {
        Debug.Log("CreateTurrets");

    }

    public void RiseTowerDefenses()
    {
        Debug.Log("RiseTowerDefenses");

    }

    public void HealTower()
    {
        Debug.Log("HealTower");

    }

}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerUpBehaviour : NetworkBehaviour
{
    //1 Minions 2 Torretas 3 Escudo de torre 4 curar torre
    private NetworkVariable<int> _puType = new NetworkVariable<int>();
    private NetworkVariable<ulong> _playerId = new NetworkVariable<ulong>();
    private NetworkVariable<ulong> _towerId = new NetworkVariable<ulong>();
    private Player _player;
   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || !NetworkManager.Singleton.IsServer) return; //Si interacciona con cualquier otro objeto con colliders nos da igual, y si no es el server entonces no tiene validez para evitar las trampas
        _playerId.Value = collision.GetComponent<NetworkObject>().NetworkObjectId;
        ExecutePowerUp();
        NetworkObject.Despawn();
        GameManager.Instance.puInScene--;

    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _puType.Value = Random.Range(1, 5);
        }

      
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void ExecutePowerUp() //Función que según el powerUp hace una cosa u otra;
    {
        Player player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_playerId.Value].GetComponent<Player>();
        var tower = NetworkManager.Singleton.SpawnManager.SpawnedObjects[player.GetTeamTower()].GetComponent<Tower>();
        Debug.Log("Torre " + tower);

        Debug.Log("Jugador detectado: " + player + " con id de " + _playerId.Value) ; //Me pilla al jugador pero no me pilla la torre

        switch (_puType.Value)
        {
            default:
                break;
            case 1:
                
                SpawnMinions();
                break;
            case 2:
               
                tower.ActivateTurrets();
                break;
            case 3:
                Debug.Log("Levantando escudo");

                tower.SetDefending(true);
                break;
            case 4:
                tower.HealTower();
                break;

        }
    }

    public void SpawnMinions()
    {
        Debug.Log("Minions");
    }


}

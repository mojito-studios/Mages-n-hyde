using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PowerUpBehaviour : NetworkBehaviour
{
    private const int MAX_TIME_PLAYER = 5;
    private bool _isTriggered = false;
    private float _currentTime = 0f;
    private int _ultimateValue = 15; //Luego ajustar
    //1 Minions 2 Flechas 3 Escudo de torre 4 curar torre
    private NetworkVariable<int> _puType = new NetworkVariable<int>();
    Player player;

   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return; 
        player = collision.GetComponent<Player>();
        _isTriggered = true;
      

    }

    private void OnTriggerExit2D(Collider2D collision) //Tanto si se sale como si el jugador se desactiva (por el tema del respawn)
    {
        Debug.Log("DESACTIVADO");
        _isTriggered = false;

    }

 
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
          
            _puType.Value = Random.Range(1, 5);
            //_puType.Value = 1; //Para probar los minions
            //_puType.Value = 2; //Para probar las flechas

        }


    }

    public void PULogic()
    {
        ExecutePowerUp();
        NetworkObject.Despawn();
        GameManager.Instance.puInScene--;
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

     void FixedUpdate()
    {
        if (_isTriggered)
        {
            _currentTime += Time.fixedDeltaTime; 

            if (_currentTime >= MAX_TIME_PLAYER)
            {
                PULogic();
                _currentTime = 0f; 
            }
        }
        else
        {
            _currentTime = 0f; 
        }
    }

    public void ExecutePowerUp() //Función que según el powerUp hace una cosa u otra;
    {
        var tower = NetworkManager.Singleton.SpawnManager.SpawnedObjects[player.GetTeamTower()].GetComponent<Tower>();
        player.SetUltiValue(_ultimateValue); 

        switch (_puType.Value)
        {
            default:
                break;
            case 1:  
                tower.SpawnMinions();
                break;
            case 2:
                tower.caster = player;
                tower.ArrowRain();
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


}

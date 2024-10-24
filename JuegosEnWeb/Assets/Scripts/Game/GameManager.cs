using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : NetworkBehaviour 
{
    //private int numPlayers = 4;
    [SerializeField] private GameObject puPrefab;
    [HideInInspector] public int puInScene = 0;
    private const int MaxPU = 5;
    public static GameManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnPUStart;
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnPUStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnPUStart;
        StartCoroutine(SpawnOverTime());
    }

    private void SpawnPU()
    {
        GameObject instance = Instantiate(puPrefab, GetRandomPosition(), Quaternion.identity);
        NetworkObject instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
        puInScene++;
        
    }
    
    private Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-8f, 8f), Random.Range(-3f, 3f), 0); //Ajustarlo luego bien al mapa esto es solo como prueba
    }

    private IEnumerator SpawnOverTime()
    {
        while(NetworkManager.Singleton.ConnectedClients.Count > 1) //No cuenta al host
        {
            yield return new WaitForSeconds(10f);
            if(puInScene < MaxPU)
                SpawnPU();
        }
    }
}

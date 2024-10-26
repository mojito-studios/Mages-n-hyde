using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : NetworkBehaviour 
{
   // private int numPlayers = 4;
    [SerializeField] private GameObject puPrefab;
    [HideInInspector] public int puInScene = 0;
    [SerializeField] private NetworkObjectPool _ObjectPool;
    private const int MaxPU = 5;
    private const int MinObj = 5;
    private const int MaxObj = 15;
    private const int MaxTimeActive = 20;
    public static GameManager Instance { get; private set; }
    private List<NetworkObject> activeObjects = new List<NetworkObject>();


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
        NetworkManager.Singleton.OnServerStarted += ActiveObjects;
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
        //if(NetworkManager.Singleton.IsServer)StartCoroutine(DespawnPU(instanceNetworkObject)); //Para que despawnee después de un tiempo. No va bien en el cliente, mirar si hacer un clientrpc
        
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0); //Ajustarlo luego bien al mapa esto es solo como prueba
    }

    private IEnumerator SpawnOverTime()
    {
        while (NetworkManager.Singleton.ConnectedClients.Count > 0) //Aparecen antes de que se conecten los clientes como tal pero da igual pq eso se arregla cuando aparezcan desde el lobby
        {
            yield return new WaitForSeconds(15f);
            if (puInScene < MaxPU) //quitar esto si no se quiere que haya más powerups en escena pq no hace falta
                SpawnPU();
        }
    }

    private IEnumerator DespawnPU(NetworkObject objectDesp)
    {
        yield return new WaitForSeconds(7f); //Ajustar tiempos si se quiere que haya más de un powerup en escena
        objectDesp.Despawn();
        puInScene--;
    }

    
    private void ActiveObjects()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnServerStarted -= ActiveObjects;
        StartCoroutine(ObjectManager());
        

    }

    private IEnumerator ObjectManager()
    {
        while (NetworkManager.Singleton.ConnectedClients.Count > 0)
        {
            if (activeObjects.Count > 0)
            {
                foreach (var networkObj in activeObjects)
                {
                    var prefab = networkObj.GetComponent<PropsBehaviour>().propSO.prefab;
                    _ObjectPool.ReturnNetworkObject(networkObj,prefab);
                    networkObj.Despawn(false);

                }
                activeObjects.Clear();
            }

            int numObjectsToActivate = Random.Range(MinObj, MaxObj);

            for (int i = 0; i < numObjectsToActivate; i++)
            {

                var networkObj = _ObjectPool.GetRandomNetworkObject(GetRandomPosition(), Quaternion.identity);
                if (networkObj != null)
                {
                    activeObjects.Add(networkObj);
                    networkObj.Spawn();

                    
                }
            }
            yield return new WaitForSeconds(MaxTimeActive);

        }
    }

    public void EndGame(string tag) //Cambiar a victoria o a derrota
    {
        Debug.Log("TorreEliminada: " + tag);
    }
}

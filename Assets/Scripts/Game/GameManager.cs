using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    [SerializeField] private Transform startPos1;
    [SerializeField] private Transform startPos2;
     public List<GameObject> prefabs = new List<GameObject>();
    public List<GameObject> props = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
     
    }

    private void HandleSceneLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            InstantiatePlayers();
            SpawnPUStart();
            ActiveObjects();
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleSceneLoadComplete;
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetPrefabIndex(Sprite sprite)
    {
        for(int i = 0;  i < prefabs.Count; i++) 
        {
            if(prefabs[i].GetComponentInChildren<SpriteRenderer>().sprite == sprite)
            {

                return i;
            }
        }
        return -1;
    }



    private void InstantiatePlayers()
    {
        int i = 0;
        int j = 0;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            PlayerData playerData = OptionsChosen.Instance.GetPlayerDataFromClientId(clientId);
            Vector3 positionSpawn;
            Quaternion orientationSpawn;

            if (playerData.team == 0) 
            {
                positionSpawn = startPos1.GetChild(i).position;
                orientationSpawn = startPos1.GetChild(i).rotation;
                i++;
            }
            else
            {
                positionSpawn = startPos2.GetChild(j).position;
                orientationSpawn = startPos1.GetChild(i).rotation;
                j++;

            }
            GameObject player = Instantiate(prefabs[playerData.prefabId], positionSpawn, orientationSpawn);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            player.GetComponent<Player>().SetSpawnPositionValue(positionSpawn);
            player.GetComponent<Player>().teamAssign = playerData.team;
            string tag = playerData.team == 0 ? "Team1Tower" : "Team2Tower";
            player.GetComponent<Player>().teamTower = GameObject.FindGameObjectWithTag(tag).GetComponent<Tower>() ;


        }
    }

    private void SpawnPUStart()
    {
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
        return new Vector3(Random.Range(-5, 10), Random.Range(-5, 5), 0); //Ajustarlo luego bien al mapa esto es solo como prueba
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

    [Rpc(SendTo.Server)]
    public void EndGameRpc(string tag) //Cambiar a victoria o a derrota
    {
        Debug.Log("TorreEliminada: " + tag);
        GameObject[] players= GameObject.FindGameObjectsWithTag("Player");
        Debug.Log(players);
        foreach (GameObject player in players)
        {
            player.GetComponent<Player>().winningTeam.Value = tag;
            player.GetComponent<Player>().EndGame();
        }
    }

}

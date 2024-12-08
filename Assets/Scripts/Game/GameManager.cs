using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : NetworkBehaviour 
{
    //PowerUps
    [SerializeField] private GameObject puPrefab;
    [HideInInspector] public int puInScene = 0;
    private const int MaxPU = 5;
    [SerializeField] private BoxCollider2D _powerUpsRange;
    private Bounds bounds;


    //Props
    [SerializeField] private NetworkObjectPool _ObjectPool;
    private List<NetworkObject> activeObjects = new List<NetworkObject>();
    private const int MinObj = 5;
    private const int MaxObj = 15;
    private const int MaxTimeActive = 20;
    private Dictionary<Vector3, bool> objectSpawningPoints = new Dictionary<Vector3, bool>();


    public static GameManager Instance { get; private set; }
    [SerializeField] private Transform startPos1;
    [SerializeField] private Transform startPos2;
     public List<GameObject> prefabs = new List<GameObject>();
    public List<GameObject> props = new List<GameObject>();

    [SerializeField] private AudioClip currentClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        bounds = _powerUpsRange.bounds;
        _powerUpsRange.enabled = false; //Desactivo para que no se interponga
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
        BackgroundMusicController.instance.SetCurrentClip(currentClip);
        
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


    private void FillDictionary()
    {
        GameObject spawningPositions = GameObject.Find("SpawningPoints");
        for(int i = 0; i < spawningPositions.transform.childCount-1; i++)
        {
            objectSpawningPoints.Add(spawningPositions.transform.GetChild(i).position, true); //TRUE = DISPONIBLE
            
        }
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
                orientationSpawn = startPos2.GetChild(j).rotation;
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

    #region PowerUps
    private void SpawnPUStart()
    {
        StartCoroutine(SpawnOverTime());
    }

    private void SpawnPU()
    {
         GameObject instance = Instantiate(puPrefab, GetPUpPosition(), Quaternion.identity);
       // GameObject instance = Instantiate(puPrefab, Vector3.zero, Quaternion.identity); Para ver bien las animaciones
        NetworkObject instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
        puInScene++;
        
    }

    private IEnumerator SpawnOverTime()
    {
        while (NetworkManager.Singleton.ConnectedClients.Count > 0) 
        {
            yield return new WaitForSeconds(50f);
            if (puInScene < MaxPU) //quitar esto si no se quiere que haya m�s powerups en escena pq no hace falta
                SpawnPU();
        }
    }

   

    private Vector3 GetPUpPosition()
    {
        Vector3 position;
        
            position = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), 0);
        
        while (Physics2D.OverlapPoint(position, LayerMask.NameToLayer("Props")) != null)
        {
            position = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), 0);
        }

        return position;
    }
    #endregion
    private Vector3 GetRandomPosition()
    {
        var aviablePoints = objectSpawningPoints.Where(kvp => kvp.Value != false).ToArray();
        return aviablePoints[Random.Range(0, aviablePoints.Length)].Key;
    }

    #region Props

    private void ActiveObjects()
    {
        FillDictionary();
        StartCoroutine(ObjectManager());
    }

    private IEnumerator ObjectManager()
    {
        while (NetworkManager.Singleton.ConnectedClients.Count > 0)
        {
            if (activeObjects.Count > 0)
            {
                for (int i = activeObjects.Count - 1; i >= 0; i--) //Al rev�s para que no me d� problemas al eliminar de la lista
                {
                    var networkObj = activeObjects[i];
                    var propsBehaviour = networkObj.GetComponent<PropsBehaviour>();
                    if (propsBehaviour.canDespawn)
                    {
                        var prefab = propsBehaviour.propSO.prefab;
                        _ObjectPool.ReturnNetworkObject(networkObj, prefab);
                        if (objectSpawningPoints.ContainsKey(networkObj.GetComponent<PropsBehaviour>().spawnPosition))
                        {
                            objectSpawningPoints[networkObj.GetComponent<PropsBehaviour>().spawnPosition] = true;
                        }
                        networkObj.Despawn(false);


                        activeObjects.RemoveAt(i);
                    }
                }
            }

         
                int remainingObjectsToActivate = MaxObj - activeObjects.Count;
                int numObjectsToActivate;

                if (remainingObjectsToActivate < MinObj) numObjectsToActivate = remainingObjectsToActivate;
                else numObjectsToActivate = Random.Range(MinObj, remainingObjectsToActivate);

                for (int i = 0; i < numObjectsToActivate; i++)
                {

                var position = GetRandomPosition();

                var networkObj = _ObjectPool.GetRandomNetworkObject(position, Quaternion.identity);

                if (networkObj != null)
                {
                    networkObj.GetComponent<PropsBehaviour>().spawnPosition = position;

                    objectSpawningPoints[position] = false;
                    activeObjects.Add(networkObj);
                    networkObj.Spawn();


                }
                else continue;
                
            }
            yield return new WaitForSeconds(MaxTimeActive);

        }
    }

    #endregion

    [Rpc(SendTo.Server)]
    public void EndGameRpc(string tag) //Cambiar a victoria o a derrota
    {
        GameObject[] players= GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<Player>().winningTeam.Value = tag;
            player.GetComponent<Player>().EndGame();
        }
    }

}
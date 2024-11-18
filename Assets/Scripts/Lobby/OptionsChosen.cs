using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsChosen : NetworkBehaviour
{
    public static OptionsChosen Instance { get; private set; }
    private NetworkList<PlayerData> playerDataList;
    private Dictionary<ulong, bool> playerReadyDictionary;
    
    private void Awake()
    {
        Instance = this;  
        DontDestroyOnLoad(gameObject); 
        playerDataList = new NetworkList<PlayerData>();
        playerReadyDictionary = new Dictionary<ulong, bool>();//diccionario que almacena cliente y ready(true) o no (false)
    }

    public void KeepTrack()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataList.Add(new PlayerData 
        {
            ClientId = clientId,
            prefabId = 0,
            team = -1
        });
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }


    [Rpc(SendTo.Server)]
    private void SetPlayerReadyServerRpc(RpcParams RpcParams = default)
    {
        playerReadyDictionary[RpcParams.Receive.SenderClientId] = true;
        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary[clientId] == true)
            {
                //algun jugador no está listo aún
                allClientsReady = false;
                break;
            }
        }
        if (allClientsReady)
        {
            Debug.Log("ready");
            NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId) 
    {
        foreach (PlayerData playerData in playerDataList)
        {
            if (playerData.ClientId == clientId)
            {
                return playerData;
            }
        }
        return default;
    }

    public int GetPlayerDataIndexFromClientId(ulong clientId) //Devuelve la posición en la lista del jugador correspondiente al id
    {
        for (int i = 0; i < playerDataList.Count; i++)
        {
            if (playerDataList[i].ClientId == clientId)
            {
                return i;
            }
        }
        return -1;
    }

    public void ChangePlayerPrefab(int prefabId) 
    {

        ChangePrefabRpc(prefabId);
    }

    [Rpc(SendTo.Server)]
    private void ChangePrefabRpc(int prefabId, RpcParams RpcParams = default)  
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(RpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataList[playerDataIndex];
        playerData.prefabId = prefabId;
        playerDataList[playerDataIndex] = playerData;
        Debug.Log("Prefab " + playerDataList[playerDataIndex].prefabId + "equipo " + playerDataList[playerDataIndex].team);
    }

    public void ChangePlayerTeam(int team)
    {
        ChangePlayerTeamRpc(team);
    }

    [Rpc(SendTo.Server)]
    private void ChangePlayerTeamRpc(int team, RpcParams RpcParams = default)
    {
        int playerDataIndex = GetPlayerDataIndexFromClientId(RpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataList[playerDataIndex];
        playerData.team = team;
        playerDataList[playerDataIndex] = playerData;
    }


}

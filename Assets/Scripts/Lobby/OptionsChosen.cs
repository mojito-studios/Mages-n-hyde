using System;
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
    public NetworkVariable<int> actualPlayersT1 = new NetworkVariable<int>();
    public NetworkVariable<int> actualPlayersT2 = new NetworkVariable<int>();
    public event Action OnReadyDisable; //Me da pereza meter un observer



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
        playerReadyDictionary[clientId] = false;
        
       
    }
   
    public override void OnNetworkSpawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            actualPlayersT1.Value = 0;
            actualPlayersT2.Value = 0;
        }
        actualPlayersT1.OnValueChanged += DebugValueT1;
        actualPlayersT2.OnValueChanged += DebugValueT2;
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
        PlayerData playerData = OptionsChosen.Instance.GetPlayerDataFromClientId(RpcParams.Receive.SenderClientId);

        if (playerData.team == -1) return;
        playerReadyDictionary[RpcParams.Receive.SenderClientId] = true;
        bool allClientsReady = true;
        Debug.Log(playerData.ClientId);
        OnReadyDisabeRpc(RpcParams.Receive.SenderClientId);
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log(playerData);
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

    [Rpc(SendTo.Everyone)]
    void OnReadyDisabeRpc(ulong clientId)
    {
        if(NetworkManager.Singleton.LocalClientId == clientId)
        {
            OnReadyDisable?.Invoke();

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

    void DebugValueT1(int oldValue, int newValue)
    {
        Debug.Log("Soy jugadores en team 1" + actualPlayersT1.Value);
    }

    void DebugValueT2(int oldValue, int newValue)
    {

        Debug.Log("Soy jugadores en team 2" + actualPlayersT2.Value);
    }

    public void AddDelete(int team, bool add)
    {
        AddDeleteRpc(team, add);
    }
    [Rpc(SendTo.Server)]
    void AddDeleteRpc(int team, bool add)
    {
        if (team == 0)
        {
            if (add)
            {
                actualPlayersT1.Value += 1;
            }
            else
            {
                actualPlayersT1.Value = Mathf.Max(0, actualPlayersT1.Value - 1);
            }
        }
        else if (team == 1)
        {
            if (add)
            {
                actualPlayersT2.Value += 1;
            }
            else
            {
                actualPlayersT2.Value = Mathf.Max(0, actualPlayersT2.Value - 1);
            }
        }
    }
}

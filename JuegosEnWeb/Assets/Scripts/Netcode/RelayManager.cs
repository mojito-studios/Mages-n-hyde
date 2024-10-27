using UIManagerSpace;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    private const int N_PLAYERS = 4; // no cuenta al host
    public UIManager UIManager;


    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn +=
            () => print($"New player {AuthenticationService.Instance.PlayerId} connected");

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(N_PLAYERS);
            
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            print(joinCode);
            //
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            //
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            print(e);
            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            //
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
            //
            //await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (RelayServiceException e)
        {
            print(e);
        }
    }
}
using UIManagerSpace;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;

public class RelayManager : MonoBehaviour
{
    private const int N_PLAYERS = 3; // no cuenta al host
    public UIManager UIManager;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn +=
            () => print($"New player {AuthenticationService.Instance.PlayerId} connected");

        await AuthenticationService.Instance.SignInAnonymouslyAsync();


    }


    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(N_PLAYERS);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            print(joinCode);

        }
        catch (RelayServiceException e)
        {
            print(e);
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
           await RelayService.Instance.JoinAllocationAsync(joinCode);

        }
        catch (RelayServiceException e)
        {
            print(e);
        }
    }
}
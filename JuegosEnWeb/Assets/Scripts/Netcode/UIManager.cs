using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIManagerSpace
{
    public class UIManager : MonoBehaviour
    {
        private string joinCode = "Enter code...";
        private const int maxConnections = 3;

        async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn +=
                () => print($"New player {AuthenticationService.Instance.PlayerId} connected");

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
            }

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            if (GUILayout.Button("Host")) StartHost();
            if (GUILayout.Button("Server")) StartServer();
            if (GUILayout.Button("Client")) StartClient(joinCode);
            joinCode = GUILayout.TextField(joinCode);
        }

        void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" :
                NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);

            GUILayout.Label("Room: " + joinCode);
        }

        private async void StartHost()
        {
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(new RelayServerData(allocation, "dtls"));
                joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                NetworkManager.Singleton.StartHost();
               // NetworkManager.Singleton.SceneManager.LoadScene("GameScene",LoadSceneMode.Single);
            }
            catch (RelayServiceException e)
            {
                print(e);
            }
        }
        private async void StartServer()
        {
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(new RelayServerData(allocation, "dtls"));
                joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                NetworkManager.Singleton.StartServer();
            }
            catch (RelayServiceException e)
            {
                print(e);
            }
        }

        private async void StartClient(string joinCode)
        {
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
                NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e)
            {
                print(e);
            }
        }
    }
}
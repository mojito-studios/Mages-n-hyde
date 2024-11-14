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
        private const int maxConnections = 4;

        async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn +=
                () => print($"New player {AuthenticationService.Instance.PlayerId} connected");

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, Screen.width-20, Screen.height-20));
            GUI.skin.button.fontSize = 120;
            GUI.skin.textField.fontSize = 120;
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
            if (GUILayout.Button("Host", GUILayout.Width(Screen.width-50), GUILayout.Height(150))) StartHost();
            GUILayout.Space(50);
            if (GUILayout.Button("Server", GUILayout.Width(Screen.width - 50), GUILayout.Height(150))) StartServer();
            GUILayout.Space(50);
            if (GUILayout.Button("Client", GUILayout.Width(Screen.width - 50), GUILayout.Height(150))) StartClient(joinCode);
            GUILayout.Space(50);
            joinCode = GUILayout.TextField(joinCode, GUILayout.Width(Screen.width - 50), GUILayout.Height(150));
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
                    .SetRelayServerData(new RelayServerData(allocation, "wss"));
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
                    .SetRelayServerData(new RelayServerData(allocation, "wss"));
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
                    .SetRelayServerData(new RelayServerData(joinAllocation, "wss"));
                NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e)
            {
                print(e);
            }
        }
    }
}
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;


namespace UIManagerSpace
{
    public class UIManager : MonoBehaviour
    {
        private string joinCode = "Enter code...";
        private int maxConnections = 4;
        [SerializeField] UnityEngine.UI.Button hostButton;
        [SerializeField] UnityEngine.UI.Button clientButton;
        [SerializeField] InputField joinCodeText;
        [SerializeField] GameObject lobbyManager;
        [SerializeField] GameObject lobbySystemManager;

         void Awake()
        {
            lobbySystemManager.SetActive(false);
            lobbyManager.SetActive(true);
        }
        async void Start()
        {
            await UnityServices.InitializeAsync();

            joinCodeText.text = joinCode;
            AuthenticationService.Instance.SignedIn +=
                () => print($"New player {AuthenticationService.Instance.PlayerId} connected");

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            //NetworkManager.Singleton.OnClientConnectedCallback += ShowLobby;
            clientButton.interactable = false;
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, Screen.width-20, Screen.height-20));
            GUI.skin.button.fontSize = 120;
            GUI.skin.textField.fontSize = 120;
            /*if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {*/
            if(NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer) //Para que se muestre en el cliente tmb el código
                StatusLabels();
            //}
            

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            if (hostButton) StartHost();
            if (clientButton) StartClient(joinCode);
            joinCode = joinCodeText.text;
        }

       /* void StartButtons()
        {
            if (GUILayout.Button("Host", GUILayout.Width(Screen.width-50), GUILayout.Height(150))) StartHost();
            GUILayout.Space(50);
          
            if (GUILayout.Button("Client", GUILayout.Width(Screen.width - 50), GUILayout.Height(150))) StartClient(joinCode);
            GUILayout.Space(50);
            joinCode = GUILayout.TextField(joinCode, GUILayout.Width(Screen.width - 50), GUILayout.Height(150));
        }*/

        void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" :
                NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);

            GUILayout.Label("Room: " + joinCode.ToUpper());
        }

        public void OnStartHostClick()
        {
            if(!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                OptionsChosen.Instance.KeepTrack();
                lobbyManager.SetActive(false);
                StartHost();
                
            }
           
           
            
        }

        public void SetJoinCode(string newJoinCode)
        {
            joinCode = newJoinCode;
            if(joinCode == null || joinCode == "" || joinCode.Contains(" ")) { clientButton.interactable = false; }
            else { clientButton.interactable = true; }
        }

        public void OnStartClientClick()
        {
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                lobbyManager.SetActive(false);
                StartClient(joinCode);
            }
                
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
                lobbySystemManager.SetActive(true);
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
                lobbySystemManager.SetActive(true);
            }
            catch (RelayServiceException e)
            {
                print(e);
                lobbyManager.SetActive(true);
            }
        }

        private void ShowLobby(ulong clientId)
        {
            if(clientId == NetworkManager.Singleton.LocalClientId)
        {
                lobbySystemManager.GetComponent<LobbySystemManager>().EnableButtons();
            }
            NetworkManager.Singleton.OnClientConnectedCallback -= ShowLobby;

        }
    }
}
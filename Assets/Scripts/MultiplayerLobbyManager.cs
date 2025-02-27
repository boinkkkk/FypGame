using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class MultiplayerLobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCodeText;  // Displays the room code
    [SerializeField] private TMP_InputField joinCodeInput; // Input field for entering a room code
    [SerializeField] private GameObject lobbyUI; // UI for lobby buttons
    [SerializeField] private GameObject lobbyRoomUI;


    private UnityTransport transport;
    private Lobby connectedLobby;
    private const int MaxPlayers = 5; // Change this as needed

    private async void Start()
    {
        // Ensure the correct UI is active at start
        lobbyUI.SetActive(true);   // Show Lobby UI
        lobbyRoomUI.SetActive(false); // Hide Lobby Room UI

        transport = FindObjectOfType<UnityTransport>(); 
        await AuthenticateUser();
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized)) {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }
    }

    private static async Task AuthenticateUser()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }


    // 🔹 Create a lobby and generate a room code
    public async void CreateLobby()
    {
        lobbyUI.SetActive(true); // Hide UI while creating

        try
        {
            // Create Relay Allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Create Lobby
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> { { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };

            connectedLobby = await Lobbies.Instance.CreateLobbyAsync("Game Lobby", MaxPlayers, options);

            roomCodeText.text = $"Room Code: {joinCode}"; // Show room code in UI

            // Assign Relay transport data
            transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            // Start as host
            NetworkManager.Singleton.StartHost();

            Debug.Log("Lobby Created with Code: " + joinCode);

            // SWITCH TO LOBBY ROOM UI  
            lobbyUI.SetActive(false);  
            lobbyRoomUI.SetActive(true);  

            // SHOW PLAYERS IN LOBBY  
            UpdateLobbyUI(connectedLobby);
        }

        catch (System.Exception e)
        {
            Debug.LogError("Failed to create lobby: " + e);
            lobbyUI.SetActive(true); // Show UI 
        }
    }

    // 🔹 Join a lobby using a room code
    public async void JoinLobby()
    {
        lobbyUI.SetActive(false); // Hide UI while joining

        string joinCode = joinCodeInput.text;
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogError("Join code is empty!");
            lobbyUI.SetActive(true);
            return;
        }

        try
        {
            // Join Relay Allocation
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Assign Relay transport data
            transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

            // Start as client
            NetworkManager.Singleton.StartClient();

            Debug.Log("Joined Lobby with Code: " + joinCode);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to join lobby: " + e);
            lobbyUI.SetActive(true);
        }
    }

    private void UpdateLobbyUI(Lobby lobby) {
    foreach (Player player in lobby.Players) {
        Debug.Log($"Player in Lobby: {player.Id}");
        // Update UI to show player characters (Add UI elements here)
    }
}

}

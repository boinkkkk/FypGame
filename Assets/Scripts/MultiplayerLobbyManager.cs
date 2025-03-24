using System;
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
using UnityEngine.UI;
using ParrelSync; // Import ParrelSync
using UnityEngine.SceneManagement;

public class MultiplayerLobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCodeText;  // Displays the room code
    [SerializeField] private TMP_InputField joinCodeInput; // Input field for entering a room code
    [SerializeField] private GameObject lobbyUI; // UI for lobby buttons
    [SerializeField] private GameObject lobbyRoomUI; //UI for lobby waiting room
    [SerializeField] private TMP_Text playerListText; // UI Text for player names


    public Button joinButton;
    private UnityTransport transport;
    private Lobby connectedLobby;
    private List<string> playerNames = new List<string>();
    private bool isPollingActive = false;
    private const int MaxPlayers = 5; // Change this as needed
    private string storedJoinCode;
    

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

        joinButton.onClick.AddListener(OnJoinButtonClicked);
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
            
            
            // storedJoinCode = joinCode;
            // PlayerPrefs.SetString("JoinCode", storedJoinCode); // Save it to PlayerPrefs
            // PlayerPrefs.Save(); // Ensure it's saved immediately

            // Create Lobby
            var options = new CreateLobbyOptions
            {
                // IsPrivate = false,
                Data = new Dictionary<string, DataObject> { { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };

            connectedLobby = await Lobbies.Instance.CreateLobbyAsync("Game Lobby", MaxPlayers, options);

            roomCodeText.text = $"Room Code: {connectedLobby.LobbyCode}"; // Show room code in UI

            // Set Host Name as Player 1
            playerNames.Add("Gogo");

            // Assign Relay transport data
            transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            // Start as host
            NetworkManager.Singleton.StartHost();

            Debug.Log("Lobby Created with Code: " + joinCode);

            await GetLobbyPlayerCount(connectedLobby.Id);
            // Start polling player count every 30 seconds
            // StartCoroutine(UpdateLobbyPlayerCountRoutine());


            // SWITCH TO LOBBY ROOM UI  
            lobbyUI.SetActive(false);  
            lobbyRoomUI.SetActive(true);  

            // SHOW PLAYERS IN LOBBY  
            UpdateLobbyUI(connectedLobby);

            // Store the lobby code before changing the scene
            PlayerPrefs.SetString("LobbyCode", connectedLobby.LobbyCode);
            PlayerPrefs.Save(); // Save PlayerPrefs
            SceneManager.LoadScene("LobbyWaitingRoom"); 


            // Start Polling for Updates
            //     isPollingActive = true;
            //     StartCoroutine(CheckForLobbyUpdates());
        }

        catch (System.Exception e)
        {
            Debug.LogError("Failed to create lobby: " + e);
            lobbyUI.SetActive(true); // Show UI 
        }
    }
    

    // 🔹 Join a lobby using a room code
    public async void JoinLobby(string LobbyCode)
    {
        try
        {
            Debug.Log("Step 1: Attempting to join lobby with code: " + LobbyCode);

            if (string.IsNullOrEmpty(LobbyCode))
            {
                Debug.LogError("Join Code is empty!");
                return;
            }
            //Query active lobbies
            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();
            foreach (Lobby foundLobby in response.Results)
            {
                Debug.Log($"Found Lobby: {foundLobby.Name}, Code: {foundLobby.LobbyCode}");
            }


            // Join the Lobby
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(LobbyCode);

            if (lobby == null)
            {
                Debug.LogError("Step 2: Failed to join lobby. The returned lobby is null.");
                return;
            }

            Debug.Log("Step 2: Successfully joined lobby. Code: " + LobbyCode);
            connectedLobby = lobby;

            // Update UI
            UpdateLobbyUI(connectedLobby);

            // Retrieve JoinCode from Lobby Data
            if (lobby.Data.TryGetValue("JoinCode", out DataObject joinCodeObj))
            {
                storedJoinCode = joinCodeObj.Value;
                Debug.Log("Retrieved Relay join code: " + storedJoinCode);
            }
            else
            {
                Debug.LogError("JoinCode not found in lobby data!");
                return;
            }

            Debug.Log("Step 3: Connecting to Relay...");
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(storedJoinCode);

            // Assign Relay transport data
            transport.SetClientRelayData(
                allocation.RelayServer.IpV4, 
                (ushort)allocation.RelayServer.Port, 
                allocation.AllocationIdBytes, 
                allocation.Key, 
                allocation.ConnectionData, 
                allocation.HostConnectionData
            );

            // Start as client
            NetworkManager.Singleton.StartClient();
            Debug.Log("Step 4: Successfully connected to Relay and started client.");

            // SWITCH TO LOBBY ROOM UI and show code 
            roomCodeText.text = $"Room Code: {connectedLobby.LobbyCode}"; // Show room code in UI
            lobbyUI.SetActive(false);  
            lobbyRoomUI.SetActive(true);  

            // Store the lobby code before changing the scene
            PlayerPrefs.SetString("LobbyCode", connectedLobby.LobbyCode);
            PlayerPrefs.Save(); // Save PlayerPrefs
            SceneManager.LoadScene("LobbyWaitingRoom"); // Change to your scene name

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e.Reason} - {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError("Unexpected Error: " + e.Message);
        }
    }

    void OnJoinButtonClicked()
    {
        string joinCode = joinCodeInput.text.Trim(); // Get input text

        if (!string.IsNullOrEmpty(joinCode))
        {
            Debug.Log("Button Pressed! Trying to join with code: " + joinCode);
            JoinLobby(joinCode);
        }
        else
        {
            Debug.LogError("Join Code is empty!");
        }
    }

    // private IEnumerator CheckForLobbyUpdates()
    // {
    //     while (isPollingActive && connectedLobby != null)
    //     {
    //         Debug.Log("Checking for lobby updates...");
    //         yield return StartCoroutine(FetchLobbyUpdates());
    //         yield return new WaitForSeconds(3f); // Poll every 3 seconds
    //     }
    // }

    // private IEnumerator FetchLobbyUpdates()
    // {
    //     if (connectedLobby == null)
    //     {
    //         Debug.LogWarning("Lobby is null, skipping update.");
    //         yield break;
    //     }

    //     Task<Lobby> fetchTask = Lobbies.Instance.GetLobbyAsync(connectedLobby.Id);
        
    //     while (!fetchTask.IsCompleted)
    //     {
    //         yield return null; // Wait for async task to complete
    //     }

    //     if (fetchTask.Exception != null)
    //     {
    //         Debug.LogError("Failed to fetch lobby updates: " + fetchTask.Exception);
    //         yield break;
    //     }

    //     connectedLobby = fetchTask.Result;
    //     Debug.Log("Lobby updated! Players: " + connectedLobby.Players.Count);

    //     UpdateLobbyUI();
    // }



    private void UpdateLobbyUI(Lobby lobby) {

        if (lobby == null)
        {
            Debug.LogError("UpdateLobbyUI called with null lobby!");
            return;
        }

        playerListText.text = ""; // Clear old player list
        foreach (Player player in lobby.Players) {
        Debug.Log($"Player in Lobby: {player.Id}");
        // Update UI to show player characters (Add UI elements here)
        }
    }
    private async Task GetLobbyPlayerCount(string lobbyId)
    {
        try
        {
            if (string.IsNullOrEmpty(lobbyId))
            {
                Debug.LogError("Lobby ID is null or empty!");
                return;
            }

            Lobby lobby = await Lobbies.Instance.GetLobbyAsync(lobbyId);
            int playerCount = lobby.Players.Count;
            Debug.Log($"Players in lobby: {playerCount}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error getting lobby: {e.Message}");
        }
    }

    private IEnumerator UpdateLobbyPlayerCountRoutine()
{
    while (connectedLobby != null)
    {
        yield return GetLobbyPlayerCount(connectedLobby.Id);
        yield return new WaitForSeconds(30f); // Wait for 10 seconds before updating again
    }
}



}

// internal class ListLobbiesOptions
// {
//     public List<QueryFilter> Filters { get; internal set; }
// }


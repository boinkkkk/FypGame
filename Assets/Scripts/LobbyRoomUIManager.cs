using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class LobbyRoomUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCodeText; // UI for showing room code
    [SerializeField] private Transform playerPanel; // Parent container for player images
    [SerializeField] private GameObject playerUIPrefab; // Prefab for player UI elements

    private string lobbyId;
    private Lobby currentLobby;
    private List<GameObject> playerUIElements = new List<GameObject>();

    async void Start()
    {
        // Get stored lobby code
        string lobbyCode = PlayerPrefs.GetString("LobbyCode", "No Code");
        roomCodeText.text = "Room Code: " + lobbyCode;

        // Try to get lobby info
        await FetchLobbyData();

        // Start polling every few seconds
        InvokeRepeating(nameof(RefreshLobbyData), 3f, 3f);
    }

    // Fetch and display players
    private async System.Threading.Tasks.Task FetchLobbyData()
    {
        try
        {
            // Fetch all lobbies to find the correct one
            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();
            foreach (Lobby lobby in response.Results)
            {
                if (lobby.LobbyCode == PlayerPrefs.GetString("LobbyCode"))
                {
                    currentLobby = lobby;
                    break;
                }
            }

            if (currentLobby == null)
            {
                Debug.LogError("Lobby not found!");
                return;
            }

            lobbyId = currentLobby.Id;
            UpdatePlayerUI();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error fetching lobby: {e.Message}");
        }
    }

    // Update UI with players
    private void UpdatePlayerUI()
    {
        // Clear previous UI elements
        foreach (GameObject obj in playerUIElements)
        {
            Destroy(obj);
        }
        playerUIElements.Clear();

        if (currentLobby.Players.Count > 2)
        {
            Debug.LogError("Lobby can't have more than 2 players!");
            return;
        }

        string localPlayerId = AuthenticationService.Instance.PlayerId;
        string hostId = currentLobby.HostId; // Get the Host's ID

        foreach (Player player in currentLobby.Players)
        {
            // Instantiate UI for each player
            GameObject playerUI = Instantiate(playerUIPrefab, playerPanel);
            playerUIElements.Add(playerUI);

            // Assign player name
            TMP_Text nameText = playerUI.GetComponentInChildren<TMP_Text>();
            if (player.Id == localPlayerId)
            {
                nameText.text = (player.Id == hostId) ? "You (Host)" : "You";
            }
            else
            {
                nameText.text = (player.Id == hostId) ? "Friend (Host)" : "Friend";
            }

            // Set position: Local player at center, the other player at -400f
            RectTransform rt = playerUI.GetComponent<RectTransform>();
            rt.anchoredPosition = (player.Id == localPlayerId) ? new Vector2(0, 0) : new Vector2(-400, 0);
        }
    }



    private async void RefreshLobbyData()
    {
        if (currentLobby == null) return;

        try
        {
            currentLobby = await Lobbies.Instance.GetLobbyAsync(lobbyId);
            UpdatePlayerUI();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error updating lobby: {e.Message}");
        }
    }

}

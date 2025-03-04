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
        // Clear old player UI elements
        foreach (GameObject obj in playerUIElements)
        {
            Destroy(obj);
        }
        playerUIElements.Clear();

        // Get the local player ID
        string localPlayerId = AuthenticationService.Instance.PlayerId;

        int playerCount = currentLobby.Players.Count;
        int middleIndex = playerCount / 2; // Find the middle position

        for (int i = 0; i < playerCount; i++)
        {
            Player player = currentLobby.Players[i];

            // Instantiate UI for each player
            GameObject playerUI = Instantiate(playerUIPrefab, playerPanel);
            playerUIElements.Add(playerUI);

            // Assign player name
            TMP_Text nameText = playerUI.GetComponentInChildren<TMP_Text>();
            nameText.text = player.Id == localPlayerId ? "You" : "Player " + (i + 1);

            // Positioning: Local player at center, others on the left
            RectTransform rt = playerUI.GetComponent<RectTransform>();
            if (player.Id == localPlayerId)
            {
                rt.anchoredPosition = new Vector2(0, 0); // Center
            }
            else
            {
                rt.anchoredPosition = new Vector2(-200 * (middleIndex - i), 0); // Spread out left
            }
        }
    }
}

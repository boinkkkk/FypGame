using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.SceneManagement;

public class LobbyRoomUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCodeText; // UI for showing room code
    [SerializeField] private Transform playerPanel; // Parent container for player images
    [SerializeField] private GameObject playerUIPrefab; // Prefab for player UI elements
    [SerializeField] private Button changeButton; // Button to change sprite
    [SerializeField] private Button readyButton; // Button to change sprite
    [SerializeField] private List<Sprite> playerSprites; // List of available player sprites
    private int currentSpriteIndex = 0;
    private Image localPlayerImage; // Reference to the image of 'You'


    private string lobbyId;
    private Lobby currentLobby;
    private List<GameObject> playerUIElements = new List<GameObject>();
    private bool isCooldownActive = false; // Cooldown flag
    private System.Threading.CancellationTokenSource debounceToken;

    async void Start()
    {
        // Get stored lobby code
        string lobbyCode = PlayerPrefs.GetString("LobbyCode", "No Code");
        roomCodeText.text = "Room Code: " + lobbyCode;

        // Try to get lobby info
        await FetchLobbyData();

        // Start polling every few seconds
        InvokeRepeating(nameof(RefreshLobbyData), 5f, 10f);

        // Assign button click event
        if (changeButton != null)
        {
            changeButton.onClick.AddListener(ChangePlayerSprite);
        }

        if (readyButton != null)
        {
            readyButton.onClick.AddListener(GoToNextScene);
        }
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
            Image playerImage = playerUI.GetComponentInChildren<Image>();

            // Retrieve player's sprite index from lobby data
            int playerSpriteIndex = 0;
            if (player.Data != null && player.Data.ContainsKey("SpriteIndex"))
            {
                playerSpriteIndex = int.Parse(player.Data["SpriteIndex"].Value);
            }

            if (player.Id == localPlayerId)
            {
                nameText.text = (player.Id == hostId) ? "You (Host)" : "You";
                localPlayerImage = playerImage; // Store reference for sprite change
                currentSpriteIndex = playerSpriteIndex; // Load saved sprite index

                if (playerSprites.Count > 0)
                {
                    localPlayerImage.sprite = playerSprites[currentSpriteIndex]; // Set initial sprite
                }

                playerUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            }
            else
            {
                nameText.text = (player.Id == hostId) ? "Friend (Host)" : "Friend";
                playerImage.sprite = playerSprites[playerSpriteIndex]; // Update sprite for friend
                playerUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(-400, 0);
            }

            // // Set position: Local player at center, the other player at -400f
            // RectTransform rt = playerUI.GetComponent<RectTransform>();
            // rt.anchoredPosition = (player.Id == localPlayerId) ? new Vector2(0, 0) : new Vector2(-400, 0);
        }
    }

    private async void ChangePlayerSprite()
    {
        if (playerSprites.Count == 0 || localPlayerImage == null) return;

        isCooldownActive = true; // Start cooldown
        changeButton.interactable = false; // Disable button

        currentSpriteIndex = (currentSpriteIndex + 1) % playerSprites.Count; // Cycle through sprites
        localPlayerImage.sprite = playerSprites[currentSpriteIndex];

        if (debounceToken != null)
        {
            debounceToken.Cancel(); // Cancel previous scheduled update
        }

        debounceToken = new System.Threading.CancellationTokenSource();
        await DelayedUpdateSprite(debounceToken.Token);

        // // Update player data in the lobby
        // await UpdatePlayerSpriteIndex(currentSpriteIndex);

        // // Refresh the lobby UI to show updated sprite for all players
        // RefreshLobbyData();

        // Set cooldown timer
        await System.Threading.Tasks.Task.Delay(3000); // 3-second delay before another click is allowed
        isCooldownActive = false; // Reset cooldown
        changeButton.interactable = true; //Re-enable button
    }

    private async System.Threading.Tasks.Task DelayedUpdateSprite(System.Threading.CancellationToken token)
    {
        try
        {
            await System.Threading.Tasks.Task.Delay(2000, token); // Wait 2 seconds
            if (!token.IsCancellationRequested)
            {
                await UpdatePlayerSpriteIndex(currentSpriteIndex);
                RefreshLobbyData();
            }
        }
        catch (System.OperationCanceledException)
        {
            // Task was canceled, do nothing
        }
    }

    private async System.Threading.Tasks.Task UpdatePlayerSpriteIndex(int newIndex)
    {
        Debug.Log($"[DEBUG] Attempting to update sprite index: {newIndex}");
        
        string localPlayerId = AuthenticationService.Instance.PlayerId;
        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>
        {
            { "SpriteIndex", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newIndex.ToString()) }
        };

        int retryAttempts = 0;
        int maxRetries = 3;  // Retry up to 5 times
        int delay = 3000;    // Start with 3s delay

        while (retryAttempts < maxRetries)
        {
            try
            {
                await Lobbies.Instance.UpdatePlayerAsync(lobbyId, localPlayerId, new UpdatePlayerOptions
                {
                    Data = playerData
                });

                Debug.Log("Sprite index updated successfully!");
                return;  // Exit loop if successful
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    Debug.LogWarning($"Rate limit exceeded! Retrying in {delay / 1000}s... ({retryAttempts + 1}/{maxRetries})");
                    await System.Threading.Tasks.Task.Delay(delay);
                    delay *= 2;  // Exponential backoff (2s → 4s → 8s...)
                    retryAttempts++;
                }
                else
                {
                    Debug.LogError($"Error updating player sprite: {e.Message}");
                    return;  // Exit if it's not a rate limit issue
                }
            }
        }

        Debug.LogError("Failed to update sprite after multiple attempts due to rate limits.");
    }

    private async void GoToNextScene()
    {
        // Store the local player's "Ready" status
        string localPlayerId = AuthenticationService.Instance.PlayerId;
        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>
        {
            { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "True") }
        };

        try
        {
            await Lobbies.Instance.UpdatePlayerAsync(lobbyId, localPlayerId, new UpdatePlayerOptions
            {
                Data = playerData
            });

            // Check if all players are ready
            await CheckAllPlayersReady();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error setting player ready: {e.Message}");
        }
    }

    private async System.Threading.Tasks.Task CheckAllPlayersReady()
    {
        try
        {
            Lobby lobby = await Lobbies.Instance.GetLobbyAsync(lobbyId);
            int readyCount = 0;

            foreach (Player player in lobby.Players)
            {
                if (player.Data.ContainsKey("Ready") && player.Data["Ready"].Value == "True")
                {
                    readyCount++;
                }
            }

            if (readyCount == lobby.Players.Count) // All players are ready
            {
                LoadGameScene();
            }
            else
            {
                Debug.Log("Waiting for all players to be ready...");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error checking ready status: {e.Message}");
        }
    }

    private void LoadGameScene()
    {
        Debug.Log("All players are ready! Loading game...");
        PlayerPrefs.SetInt("PlayerSpriteIndex", currentSpriteIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("LevelSample"); // Change to your actual game scene
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

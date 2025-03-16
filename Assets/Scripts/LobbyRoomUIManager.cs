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
    [SerializeField] private Button readyButton; // Ready Button
    [SerializeField] private Button startGameButton; // Start Game Button
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
        startGameButton.gameObject.SetActive(false); //Hide StartGameButton initially
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

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame); // Attach event to button
        }
        // InvokeRepeating(nameof(RefreshLobbyAndCheckReady), 5f, 5f); // Every 5 sec
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
        int maxRetries = 4;  // Retry up to 5 times
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
            Debug.Log("✅ Player marked as ready. Checking if all players are ready...");

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
        // if (currentLobby == null) return;

        // bool allPlayersReady = true;

        // foreach (Player player in currentLobby.Players)
        // {
        //     if (!player.Data.ContainsKey("Ready") || player.Data["Ready"].Value != "True")
        //     {
        //         allPlayersReady = false;
        //         break;
        //     }
        // }

        // if (allPlayersReady)
        // {
        //     Debug.Log("✅ All players are ready! Loading game...");
        //     SceneManager.LoadScene("LevelSample");
        // }
        // else
        // {
        //     Debug.Log("❌ Not all players are ready yet.");
        // }

        if (string.IsNullOrEmpty(lobbyId))
        {
            Debug.LogError("lobbyId is null or empty! Cannot check ready status.");
            return;
        }

        try
        {
            Lobby lobby = await Lobbies.Instance.GetLobbyAsync(lobbyId);

            if (lobby == null)
            {
                Debug.LogError("Lobby is null! Cannot check ready status.");
                return;
            }

            if (lobby.Players == null || lobby.Players.Count == 0)
            {
                Debug.LogError("No players in lobby! Cannot check ready status.");
                return;
            }

            int readyCount = 0;

            foreach (Player player in lobby.Players)
            {
                if (player.Data != null && player.Data.ContainsKey("Ready") && player.Data["Ready"].Value == "True")
                {
                    readyCount++;
                }
            }

            Debug.Log($"Players Ready: {readyCount}/{lobby.Players.Count}");

            if (readyCount == lobby.Players.Count) // All players are ready
            {
                if (AuthenticationService.Instance.PlayerId == lobby.HostId) // if this player is the host, show "Start Game" button
                {
                    startGameButton.gameObject.SetActive(true);
                }
                else
                {
                    Debug.Log("Waiting for the host to start the game...");
                }
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

    private async void StartGame()
    {
        if (AuthenticationService.Instance.PlayerId != currentLobby.HostId)
        {
            Debug.LogWarning("⚠️ Only the host can start the game!");
            return;
        }

        try
        {
            Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
            {
                { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, "True") }
            };

            await Lobbies.Instance.UpdateLobbyAsync(lobbyId, new UpdateLobbyOptions
            {
                Data = lobbyData
            });

            Debug.Log("🎮 Game started! Notifying all players...");
            NetworkManager.Singleton.SceneManager.LoadScene("LevelSample", LoadSceneMode.Single);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"❌ Error starting game: {e.Message}");
        }
    }


    private async void RefreshLobbyAndCheckReady()
    {
        if (currentLobby == null) return;

        try
        {
            currentLobby = await Lobbies.Instance.GetLobbyAsync(lobbyId);
            await CheckAllPlayersReady(); // Check status after fetching data
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.RateLimited)
            {
                Debug.LogWarning("⚠️ Rate limit exceeded! Slowing down requests.");
                CancelInvoke(nameof(RefreshLobbyAndCheckReady));
                InvokeRepeating(nameof(RefreshLobbyAndCheckReady), 10f, 10f); // Increase delay
            }
            else
            {
                Debug.LogError($"Error updating lobby: {e.Message}");
            }
        }
    }


    private void LoadGameScene()
    {
        Debug.Log("All players are ready! Loading game...");
        PlayerPrefs.SetInt("PlayerSpriteIndex", currentSpriteIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("LevelSample"); // Change to your actual game scene
    }









    // private async void RefreshLobbyData()
    // {
    //     if (currentLobby == null) return;

    //     try
    //     {
    //         currentLobby = await Lobbies.Instance.GetLobbyAsync(lobbyId);
    //         UpdatePlayerUI();
    //         // await CheckAllPlayersReady();
    //     }
    //     catch (LobbyServiceException e)
    //     {
    //         Debug.LogError($"Error updating lobby: {e.Message}");
    //     }
    // }

    private Dictionary<string, string> lastReadyStates = new Dictionary<string, string>(); // Store last known ready states

    private async void RefreshLobbyData()
    {
        if (currentLobby == null || isCooldownActive) return;

        try
        {
            isCooldownActive = true;
            Lobby updatedLobby = await Lobbies.Instance.GetLobbyAsync(lobbyId);

            // Check for "Ready" state changes
            foreach (Player player in updatedLobby.Players)
            {
                string playerId = player.Id;
                string newReadyState = (player.Data != null && player.Data.ContainsKey("Ready")) ? player.Data["Ready"].Value : "False";

                if (lastReadyStates.ContainsKey(playerId))
                {
                    // Detect if the player's "Ready" state changed
                    if (lastReadyStates[playerId] != newReadyState)
                    {
                        // await CheckAllPlayersReady();
                        Debug.Log($"🔔 Player {playerId} is now {(newReadyState == "True" ? "READY" : "NOT READY")}");
                    }
                }

                // Update stored state
                lastReadyStates[playerId] = newReadyState;
            }

            if (currentLobby.Data.ContainsKey("GameStarted") && currentLobby.Data["GameStarted"].Value == "True")
            {
                Debug.Log("🎮 Game start detected! Loading scene...");
                LoadGameScene();
            }

            currentLobby = updatedLobby;
            UpdatePlayerUI();
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.RateLimited)
            {
                Debug.LogWarning("⚠️ Rate limit exceeded! Increasing refresh delay.");
                CancelInvoke(nameof(RefreshLobbyData));
                InvokeRepeating(nameof(RefreshLobbyData), 15f, 15f);
            }
            else
            {
                Debug.LogError($"Error updating lobby: {e.Message}");
            }
        }
        finally
        {
            await System.Threading.Tasks.Task.Delay(5000);
            isCooldownActive = false;
        }
    }


}

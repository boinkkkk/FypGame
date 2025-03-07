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
    [SerializeField] private Button changeButton; // Button to change sprite
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
        InvokeRepeating(nameof(RefreshLobbyData), 5f, 5f);

        // Assign button click event
        if (changeButton != null)
        {
            changeButton.onClick.AddListener(ChangePlayerSprite);
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

    private void ChangePlayerSprite()
    {
        if (playerSprites.Count == 0 || localPlayerImage == null) return;

        isCooldownActive = true; // Start cooldown
        // changeButton.interactable = false; // Disable button

        currentSpriteIndex = (currentSpriteIndex + 1) % playerSprites.Count; // Cycle through sprites
        localPlayerImage.sprite = playerSprites[currentSpriteIndex];

        if (debounceToken != null)
        {
            debounceToken.Cancel(); // Cancel previous scheduled update
        }

        debounceToken = new System.Threading.CancellationTokenSource();
        _ = DelayedUpdateSprite(debounceToken.Token);

        // // Update player data in the lobby
        // await UpdatePlayerSpriteIndex(currentSpriteIndex);

        // // Refresh the lobby UI to show updated sprite for all players
        // RefreshLobbyData();

        // // Set cooldown timer
        // await System.Threading.Tasks.Task.Delay(3000); // 3-second delay before another click is allowed
        // isCooldownActive = false; // Reset cooldown
        // // changeButton.interactable = true; //Re-enable button
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
        try
        {
            string localPlayerId = AuthenticationService.Instance.PlayerId;
            Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>
            {
                { "SpriteIndex", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newIndex.ToString()) }
            };

            await Lobbies.Instance.UpdatePlayerAsync(lobbyId, localPlayerId, new UpdatePlayerOptions
            {
                Data = playerData
            });

            Debug.Log("Sprite index updated successfully!");

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error updating player sprite: {e.Message}");
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

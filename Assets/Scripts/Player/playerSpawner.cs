using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class playerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Prefab for the player object
    [SerializeField] private Transform spawnPoint1;  // Spawn point for player 1
    [SerializeField] private Transform spawnPoint2;  // Spawn point for player 2
    [SerializeField] private List<Sprite> playerSprites; // List of available sprites

    private async void Start()
    {
        await FetchLobbyData();
    }

    private async System.Threading.Tasks.Task FetchLobbyData()
    {
        try
        {
            // Fetch the current lobby
            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();
            Lobby currentLobby = null;

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
                Debug.LogError("Lobby not found in game scene!");
                return;
            }

            string localPlayerId = AuthenticationService.Instance.PlayerId;
            string hostId = currentLobby.HostId;

            int index = 0;
            foreach (Player player in currentLobby.Players)
            {
                // Retrieve sprite index
                int spriteIndex = 0;
                if (player.Data != null && player.Data.ContainsKey("SpriteIndex"))
                {
                    spriteIndex = int.Parse(player.Data["SpriteIndex"].Value);
                }

                // Instantiate player object
                GameObject newPlayer = Instantiate(playerPrefab, (index == 0) ? spawnPoint1.position : spawnPoint2.position, Quaternion.identity);
                Image playerImage = newPlayer.GetComponentInChildren<Image>(); // Assuming your prefab has an Image component

                // Assign sprite
                if (playerSprites.Count > spriteIndex)
                {
                    playerImage.sprite = playerSprites[spriteIndex];
                }

                index++;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error fetching lobby data in game scene: {e.Message}");
        }
    }
}

using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerPrefab; // Assign in Unity Inspector
    private Lobby currentLobby;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnPlayers();
        }
    }

    private async void SpawnPlayers()
    {
        // Check if the lobby code exists
        string lobbyCode = PlayerPrefs.GetString("LobbyCode");
        
        if (string.IsNullOrEmpty(lobbyCode))
        {
            Debug.LogError("Lobby code is missing in PlayerPrefs.");
            return;
        }

        // Fetch the lobby data using QueryLobbiesAsync
        QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();

        // Debug: Log the number of lobbies fetched
        Debug.Log($"Number of lobbies fetched: {response.Results.Count}");

        bool lobbyFound = false;

        // Search through the lobbies to find the matching one
        foreach (Lobby lobby in response.Results)
        {
            if (lobby.LobbyCode == lobbyCode)
            {
                currentLobby = lobby;
                lobbyFound = true;
                break;
            }
        }

        if (!lobbyFound)
        {
            Debug.LogError($"Lobby with code {lobbyCode} not found!");
            return;
        }

        // Debug: Confirm lobby found
        Debug.Log($"Lobby found: {currentLobby.Name} with {currentLobby.Players.Count} players.");

        // Spawn each player at a different position
        Vector3[] spawnPositions = { new Vector3(-3, 0, 0), new Vector3(3, 0, 0) };
        int index = 0;

        foreach (Player player in currentLobby.Players)
        {
            GameObject newPlayer = Instantiate(playerPrefab, spawnPositions[index], Quaternion.identity);
            NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();

            // Assign ownership correctly using Netcode client IDs
            ulong clientId = (ulong)index;  // TEMP FIX: Assign IDs sequentially (not ideal for multi-client)
            networkObject.SpawnAsPlayerObject(clientId);

            index++;
        }
    }

    public void RespawnPlayer(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("Player object is null during respawn.");
            return;
        }

        // Try to find and respawn player
        string lobbyCode = PlayerPrefs.GetString("LobbyCode");
        if (string.IsNullOrEmpty(lobbyCode))
        {
            Debug.LogError("LobbyCode missing during respawn, can't fetch lobby.");
            return;
        }

        // Respawn logic (e.g., teleport player to the spawn point or restart the scene)
        StartCoroutine(RespawnCoroutine(player, lobbyCode));
    }

    private IEnumerator RespawnCoroutine(GameObject player, string lobbyCode)
    {
        // Make sure we have the lobby loaded or re-fetch it
        yield return new WaitForSeconds(1);  // Wait for a short time before respawning

        // Fetch the lobby asynchronously using Task
        Task<QueryResponse> queryTask = Task.Run(async () =>
        {
            return await Lobbies.Instance.QueryLobbiesAsync();
        });

        // Wait for the async task to complete
        yield return new WaitUntil(() => queryTask.IsCompleted);

        // Check if the task completed successfully
        if (queryTask.IsFaulted || queryTask.Result == null)
        {
            Debug.LogError("Lobby fetching failed or returned null.");
            yield break;
        }

        // Retrieve the lobby from the result
        QueryResponse response = queryTask.Result;
        Lobby lobby = null;
        
        foreach (Lobby lobbyItem in response.Results)
        {
            if (lobbyItem.LobbyCode == lobbyCode)
            {
                lobby = lobbyItem;
                break;
            }
        }

        if (lobby == null)
        {
            Debug.LogError("Lobby not found during respawn.");
            yield break;
        }

        // Find an available spawn point for the player
        Vector3 respawnPosition = new Vector3(0, 0, 0);  // Choose your respawn position

        // Move the player to the respawn position
        player.transform.position = respawnPosition;
        player.SetActive(true);  // Ensure the player is active

        // Optionally, you could trigger any animation or other effects here
        Debug.Log("Player respawned at position: " + respawnPosition);
    }
}

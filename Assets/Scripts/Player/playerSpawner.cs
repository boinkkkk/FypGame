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
        // Fetch the lobby data
        string lobbyCode = PlayerPrefs.GetString("LobbyCode");
        QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();
        
        foreach (Lobby lobby in response.Results)
        {
            if (lobby.LobbyCode == lobbyCode)
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
}

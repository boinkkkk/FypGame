using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using UnityEngine.SceneManagement; 

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerPrefab; // Assign in Unity Inspector
    private Lobby currentLobby;
    private bool playersSpawned = false;  // Add this flag to track if players are already spawned
    private string lobbyId;
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("Start method is running");

        
        // string lobbyCode = PlayerPrefs.GetString("LobbyCode");
        // Debug.Log("lobbycode is " + lobbyCode);

        // // Fetch lobby code at the start of game and after sccene transition
        // lobbyId = PlayerPrefs.GetString("LobbyId", "");
        // Debug.Log("this is the lobby id:" +lobbyId);
        // if (string.IsNullOrEmpty(lobbyId))
        // {
        //     Debug.LogError("Lobby code not found!");
        // }

        string joinCode = PlayerPrefs.GetString("JoinCode");
        if (!string.IsNullOrEmpty(joinCode))
        {
            Debug.Log("Retrieved Join Code: " + joinCode);
            // Use the joinCode as needed
        }
        else
        {
            Debug.LogError("Join Code not found!");
        }

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string lobbyCode = PlayerPrefs.GetString("LobbyCode");
        Debug.Log("lobbycode is " + lobbyCode);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && !playersSpawned)
        {
            SpawnPlayers();
            // StartCoroutine(SpawnPlayersAfterSceneLoad());
            playersSpawned = true;  // Set the flag so players don't spawn again
        }
    }

    private IEnumerator SpawnPlayersAfterSceneLoad()
    {
        yield return new WaitForSeconds(1); // Give time for the scene to load

        SpawnPlayers();
    }

    // private async void SpawnPlayers()
    // {
    //     // Check if the lobby code exists
    //     string lobbyCode = PlayerPrefs.GetString("LobbyCode");
        
    //     if (string.IsNullOrEmpty(lobbyCode))
    //     {
    //         Debug.LogError("Lobby code is missing in PlayerPrefs.");
    //         return;
    //     }

    //     // Fetch the lobby data using QueryLobbiesAsync
    //     QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();

    //     // Debug: Log the number of lobbies fetched
    //     Debug.Log($"Number of lobbies fetched: {response.Results.Count}");

    //     bool lobbyFound = false;

    //     // Search through the lobbies to find the matching one
    //     foreach (Lobby lobby in response.Results)
    //     {
    //         if (lobby.LobbyCode == lobbyCode)
    //         {
    //             currentLobby = lobby;
    //             lobbyFound = true;
    //             break;
    //         }
    //     }

    //     if (!lobbyFound)
    //     {
    //         Debug.LogError($"Lobby with code {lobbyCode} not found!");
    //         return;
    //     }

    //     // Debug: Confirm lobby found
    //     Debug.Log($"Lobby found: {currentLobby.Name} with {currentLobby.Players.Count} players.");

    //     // Spawn each player at a different position
    //     Vector3[] spawnPositions = { new Vector3(-11, 0, 0), new Vector3(-9, 0, 0) };
    //     int index = 0;

    //     foreach (Player player in currentLobby.Players)
    //     {
    //         GameObject newPlayer = Instantiate(playerPrefab, spawnPositions[index], Quaternion.identity);
    //         NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();

    //         // // Assign ownership correctly using Netcode client IDs
    //         // ulong clientId = (ulong)index;  // TEMP FIX: Assign IDs sequentially (not ideal for multi-client)

    //         // Get the correct Client ID from Netcode
    //         ulong clientId = NetworkManager.Singleton.ConnectedClientsList[index].ClientId;
    //         networkObject.SpawnAsPlayerObject(clientId);

    //         index++;
    //     }
    // }

    private async void SpawnPlayers()
    {
        string lobbyCode = PlayerPrefs.GetString("LobbyCode");
        
        if (string.IsNullOrEmpty(lobbyCode))
        {
            Debug.LogError("Lobby code is missing in PlayerPrefs.");
            return;
        }

        await Task.Delay(2000);
        QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync();
        Debug.Log($"Lobby query success, found {response.Results.Count} lobbies.");
        bool lobbyFound = false;
        
        foreach (Lobby lobby in response.Results)
        {
            // Debug.Log("preparing to run response!!!");

            if (lobby.LobbyCode == lobbyCode)
            {   
                
                currentLobby = lobby;
                lobbyFound = true;
                break;
            }
        }

        if (!lobbyFound)
        {
            // Debug.LogError($"Lobby with code {lobbyCode} not found!");
            // return;
        }

        // Debug.Log($"Lobby found: {currentLobby.Name} with {currentLobby.Players.Count} players.");

        Vector3[] spawnPositions = { new Vector3(-11, 0, 0), new Vector3(-9, 0, 0) };
        
        int index = 0;

        // foreach (Player lobbyPlayer in currentLobby.Players)
        // {
        //     ulong clientId = NetworkManager.Singleton.ConnectedClientsList[index].ClientId;

        //     // **Check if the player already exists**
        //     foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        //     {
        //         if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
        //         {
        //             Debug.Log($"Player {clientId} already exists, skipping spawn.");
        //             index++;
        //             return;
        //         }
        //     }

        //     // **Spawn player only if it doesn’t already exist**
        //     GameObject newPlayer = Instantiate(playerPrefab, spawnPositions[index], Quaternion.identity);
        //     NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();
        //     networkObject.SpawnAsPlayerObject(clientId);

        //     // Make sure player persists across scenes
        //     DontDestroyOnLoad(newPlayer);

        //     index++;
        // }


        // foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        // {
        //     if (index >= 2) break; // Limit to one host and one client

        //     ulong clientId = client.ClientId;
        //     if (!IsPlayerAlreadySpawned(clientId))
        //     {
        //         GameObject newPlayer = Instantiate(playerPrefab, spawnPositions[index], Quaternion.identity);
        //         NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();
        //         networkObject.SpawnAsPlayerObject(clientId);
        //         DontDestroyOnLoad(newPlayer);
        //         index++;
        //     }
        // }
        
        GameObject GetPlayerObject(ulong clientId)
        {
            foreach (var networkObject in FindObjectsOfType<NetworkObject>())
            {
                if (networkObject.OwnerClientId == clientId) // Check ownership
                {
                    return networkObject.gameObject; // Return the player's GameObject
                }
            }
            return null; // Return null if the player is not found
        }


        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;

            if (index < 2) // Only process up to two players
            {
                if (IsPlayerAlreadySpawned(clientId))
                {
                    // If player exists, relocate them
                    GameObject existingPlayer = GetPlayerObject(clientId);
                    if (existingPlayer != null)
                    {
                        existingPlayer.transform.position = spawnPositions[index];
                    }
                }
                else
                {
                    // Spawn new player
                    GameObject newPlayer = Instantiate(playerPrefab, spawnPositions[index], Quaternion.identity);
                    NetworkObject networkObject = newPlayer.GetComponent<NetworkObject>();
                    networkObject.SpawnAsPlayerObject(clientId);
                    DontDestroyOnLoad(newPlayer);
                }

                index++;
            }
        }
    }

    private bool IsPlayerAlreadySpawned(ulong clientId)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (client.PlayerObject != null && client.PlayerObject.IsSpawned && client.ClientId == clientId)
            {
                Debug.Log($"Player {clientId} already spawned.");
                return true;
            }
        }
        return false;
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

    public void LoadNextScene()
    {
        if (IsServer) // Only the server should load the scene
        {
            // foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            // {
            //     if (client.PlayerObject != null)
            //     {
            //         client.PlayerObject.Despawn(true);
            //     }
            // }

            NetworkManager.Singleton.SceneManager.LoadScene("Level2", LoadSceneMode.Single);
            // Wait for the scene to be fully loaded before proceeding
            // StartCoroutine(WaitForSceneToLoadAndSpawnNewPlayer());
        }
    }

    public void LoadNextScene2()
    {
        if (IsServer) // Only the server should load the scene
        {
            // foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            // {
            //     if (client.PlayerObject != null)
            //     {
            //         client.PlayerObject.Despawn(true);
            //     }
            // }

            NetworkManager.Singleton.SceneManager.LoadScene("Level3", LoadSceneMode.Single);
            // Wait for the scene to be fully loaded before proceeding
            // StartCoroutine(WaitForSceneToLoadAndSpawnNewPlayer());
        }
    }

    // private IEnumerator WaitForSceneToLoadAndSpawnNewPlayer()
    // {
    //     // Wait until the new scene is fully loaded
    //     yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "LevelSample2");

    //     // Spawn the new player once the scene is loaded
    //     // SpawnPlayers();

    //     // Once the new player is spawned, despawn the old player
    //     DespawnPreviousPlayer();
    // }

    // private void DespawnPreviousPlayer()
    // {
    //     // Despawn the previous player after the new player is spawned
    //     foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
    //     {
    //         Debug.Log ("Despawning prev player");
    //         if (client.PlayerObject != null)
    //         {
    //             client.PlayerObject.Despawn(true);
    //         }
    //     }
    // }
}

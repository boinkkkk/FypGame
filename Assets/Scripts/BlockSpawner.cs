using UnityEngine;
using Unity.Netcode;

public class BlockSpawner : NetworkBehaviour
{
    public GameObject blockPrefab; // Assign in Inspector
    public float spawnInterval = 2f; // Blocks spawn every 2 seconds

    void Start()
    {
        if (IsServer) // Ensure only the server spawns blocks
        {
            InvokeRepeating(nameof(SpawnBlock), 0f, spawnInterval);
        }
    }

    void SpawnBlock()
    {
        GameObject newBlock = Instantiate(blockPrefab, transform.position, Quaternion.identity);

        // Ensure the block has a NetworkObject component
        NetworkObject networkObject = newBlock.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true); // Spawns the block and syncs it across all clients
        }
    }
}

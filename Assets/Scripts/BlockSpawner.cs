using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    public GameObject blockPrefab; // Assign your block prefab in the Inspector
    public float spawnInterval = 2f; // Blocks spawn every 2 seconds

    void Start()
    {
        InvokeRepeating(nameof(SpawnBlock), 0f, spawnInterval); // Call SpawnBlock every 2s
    }

    void SpawnBlock()
    {
        Instantiate(blockPrefab, transform.position, Quaternion.identity);
    }
}

using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class PlayerDeathTest : NetworkBehaviour
{
    public Rigidbody2D rb;
    public GameObject respawnPoint; // Set this in Unity
    public float deathDelay = 1.5f;
    private bool isRespawning = false;
    private Animator animator;
    private float fallThreshold = -9f;
    private greyPlayerMovement movementScript;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        movementScript = GetComponent<greyPlayerMovement>();  // Reference the movement script
    }

    private void Update()
    {
        if(transform.position.y < fallThreshold && !isRespawning)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !isRespawning)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        animator.SetTrigger("Die");

        // Disable movement and gravity
        movementScript.enabled = false;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        StartCoroutine(RespawnCoroutine());

        // If the server detects death, restart the scene for everyone
        // if (IsServer)
        // {
        //     StartCoroutine(ResetSceneForAllPlayers());
        // }
    }

    private IEnumerator RespawnCoroutine()
    {
        isRespawning = true;

        // Wait for death delay before respawning
        yield return new WaitForSeconds(deathDelay);

        // Respawn the player
        transform.position = respawnPoint.transform.position;

        // Reset animations and re-enable controls
        animator.ResetTrigger("Die");
        animator.Play("IdleState");  // Ensure it's back to idle

        movementScript.enabled = true;  // Re-enable movement
        rb.gravityScale = 7;  // Restore gravity

        isRespawning = false;
    }

    // [ServerRpc(RequireOwnership = false)]
    // private void ResetSceneForAllPlayersServerRpc()
    // {
    //     NetworkManager.Singleton.SceneManager.LoadScene("LevelSample", LoadSceneMode.Single);

    //     // if (NetworkManager.Singleton.IsServer)
    //     // {
    //     //     NetworkManager.Singleton.SceneManager.LoadScene("LevelSample", LoadSceneMode.Single);
    //     // }
    // }

    // private IEnumerator ResetSceneForAllPlayers()
    // {
    //     yield return new WaitForSeconds(deathDelay);
    //     ResetSceneForAllPlayersServerRpc();
    // }
}
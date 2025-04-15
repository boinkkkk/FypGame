using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class PlayerDeath : NetworkBehaviour
{
    public Rigidbody2D rb;
    public GameObject respawnPoint; // Set this in Unity
    public float deathDelay = 1.5f;
    public AudioClip deathSound;
    private bool isRespawning = false;
    private Animator animator;
    private float fallThreshold = -12f;
    private NewPlayerMovement movementScript;
    private RedButtonActivation redButton;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        movementScript = GetComponent<NewPlayerMovement>();  // Reference the movement script

        redButton = FindObjectOfType<RedButtonActivation>(); // Find the button in the scene
    }

    private void Update()
    {
        if (IsServer) // Ensure this check runs only on the server
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                TriggerDeathForAllPlayers();
            }
        }

        if(transform.position.y < fallThreshold && !isRespawning)
        {
            NotifyDeathServerRpc();
            // Die();
        }
    }

    private void TriggerDeathForAllPlayers()
    {
        if (IsServer)
        {
            KillAllPlayersClientRpc();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !isRespawning)
        {
            
            NotifyDeathServerRpc();
        }
    }

    // Notify server that a player has died
    [ServerRpc(RequireOwnership = false)]
    private void NotifyDeathServerRpc()
    {
        Debug.Log($"Server received death notification from {OwnerClientId}");
        KillAllPlayersClientRpc(); // Call ClientRpc to kill all players
    }

    // Make all players die
    [ClientRpc]
    private void KillAllPlayersClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("killing all players");
        audioSource.PlayOneShot(deathSound);
        // Die();
        foreach(var player in FindObjectsOfType<PlayerDeath>())
        {
            player.Die();
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

        yield return new WaitForSeconds(0.5f); // Small delay to avoid instant death when respawning
        isRespawning = false;

       // Reset the Red Button for all players
        if (IsServer && redButton != null)
        {
            redButton.ResetButtonServerRpc();
        }
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
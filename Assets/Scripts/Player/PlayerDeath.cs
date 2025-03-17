// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class PlayerDeath : MonoBehaviour
// {
//     public Rigidbody2D rb;
//     public GameObject respawnPoint; // Optional: For respawning the player
//     public float deathDelay = 1.5f; // Delay before death effect
//     Animator animator;


//  void Start()
//     {
//         rb = GetComponent<Rigidbody2D>(); // Get the RigidBody
//         animator = GetComponent<Animator>();
//     }
//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         // Check if the player collides with the spikes
//         if (other.CompareTag("Enemy"))
//         {
//             Die();
//         }
//     }

//     private void Die()
//     {
//         // Example: Display a death effect, disable controls, or reload the scene
//         Debug.Log("Player Died!");

//         animator.SetTrigger("Die");
//         Invoke("RestartScene", deathDelay);

//         // Disable movement or player controls (optional)
//         GetComponent<NewPlayerMovement>().enabled = false;
        
//         // Optional: Restart the scene (uncomment to use)
//         // SceneManager.LoadScene(SceneManager.GetActiveScene().LevelSample);
//         // SceneManager.LoadSceneAsync("LevelSample");
        
//         // Optional: Respawn the player (if you have a respawn point)
//         // animator.SetTrigger("Die");
//         // StartCoroutine(Respawn());
         
//     }
//     private void RestartScene()
//     {
//         // Reload the current scene
//         SceneManager.LoadSceneAsync("LevelSample");
//         // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//         StartCoroutine(Respawn());
//     }

//     // Optional respawn coroutine
//     private IEnumerator Respawn()
//     {
//         animator.SetTrigger("Die");
//         // Disable the player temporarily
//         // gameObject.SetActive(false);

//         // Wait for the death delay
//         yield return new WaitForSeconds(deathDelay);

//         // Move to the respawn point and re-enable the player
//         transform.position = respawnPoint.transform.position;
//         gameObject.SetActive(true);
//     }
// }

using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class PlayerDeath : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject respawnPoint; // Set this in Unity
    public float deathDelay = 1.5f;
    private bool isRespawning = false;
    private Animator animator;
    private NewPlayerMovement movementScript;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        movementScript = GetComponent<NewPlayerMovement>();  // Reference the movement script
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
}

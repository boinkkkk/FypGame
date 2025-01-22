using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    public Rigidbody2D rb;
    public GameObject respawnPoint; // Optional: For respawning the player
    public float deathDelay = 1.5f; // Delay before death effect
    Animator animator;


 void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the RigidBody
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player collides with the spikes
        if (other.CompareTag("Enemy"))
        {
            Die();
        }
    }

    private void Die()
    {
        // Example: Display a death effect, disable controls, or reload the scene
        Debug.Log("Player Died!");

        animator.SetTrigger("Die");
        Invoke("RestartScene", deathDelay);

        // Disable movement or player controls (optional)
        GetComponent<greyPlayerMovement>().enabled = false;
        
        // Optional: Restart the scene (uncomment to use)
        // SceneManager.LoadScene(SceneManager.GetActiveScene().LevelSample);
        // SceneManager.LoadSceneAsync("LevelSample");
        
        // Optional: Respawn the player (if you have a respawn point)
        // StartCoroutine(Respawn());
         
    }
    private void RestartScene()
    {
        // Reload the current scene
        SceneManager.LoadSceneAsync("LevelSample");
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Optional respawn coroutine
    private IEnumerator Respawn()
    {
        // Disable the player temporarily
        gameObject.SetActive(false);

        // Wait for the death delay
        yield return new WaitForSeconds(deathDelay);

        // Move to the respawn point and re-enable the player
        transform.position = respawnPoint.transform.position;
        gameObject.SetActive(true);
    }
}
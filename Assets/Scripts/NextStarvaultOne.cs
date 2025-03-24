using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class NextStarvaultOne : NetworkBehaviour
{
    // public string lobbyCode; // Set this from the Game Manager or UI

    public void OnLevelComplete()
    {
        // // Store the lobby code before switching scenes
        // PlayerPrefs.SetString("LobbyCode", lobbyCode);
        
        // Get stored lobby code
        string lobbyCode = PlayerPrefs.GetString("LobbyCode", "No Code");
        PlayerPrefs.Save(); // Save PlayerPrefs

        // Load the next scene
        SceneManager.LoadScene("LevelSample2"); // Change to your actual game scene
    }
}

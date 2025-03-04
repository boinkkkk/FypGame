using UnityEngine;
using TMPro;

public class LobbyRoomUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCodeText; // Assign this in Inspector

    void Start()
    {
        // Retrieve the stored lobby code
        string lobbyCode = PlayerPrefs.GetString("LobbyCode", "No Code");
        
        // Display in UI
        if (roomCodeText != null)
        {
            roomCodeText.text = "Room Code: " + lobbyCode;
        }
        else
        {
            Debug.LogError("roomCodeText is not assigned in the Inspector!");
        }
    }
}

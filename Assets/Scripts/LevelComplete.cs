using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class LevelComplete : NetworkBehaviour
{
    [SerializeField] GameObject CongratsPanel;
    // [SerializeField] Button testButton;
    [SerializeField] Button closeButton;
    private int playersInTrigger = 0; // Track the number of players inside the trigger
    // [SerializeField] GameObject StarFragment;

    // Start is called before the first frame update
    void Start()
    {
        CongratsPanel.SetActive(false);

        // testButton.onClick.AddListener(OnTestButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called when the test button is clicked
    // private void OnTestButtonClicked()
    // {
    //     if (IsServer)
    //     {
    //         // If the host clicks, directly show UI on all clients
    //         ShowUiClientRpc();
    //     }
    //     else
    //     {
    //         // If a client clicks, request the server to show the UI
    //         RequestShowUiServerRpc();
    //     }
    // }
    private void OnCloseButtonClicked()
    {
        if (IsServer)
        {
            // If the host clicks, directly show UI on all clients
            CloseUiClientRpc();
        }
        else
        {
            // If a client clicks, request the server to show the UI
            RequestCloseUiServerRpc();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // if (other.gameObject.CompareTag("Player"))
        // {
        //     if (IsServer)
        //     {
        //         ShowUiClientRpc(); // Host can directly trigger UI
        //     }
        //     else
        //     {
        //         RequestShowUiServerRpc(); // Client requests server to trigger UI
        //     }

        // }
        if (other.gameObject.CompareTag("Player"))
            {
                playersInTrigger++; // Increase count when a player enters

                if (playersInTrigger >= 2) // Ensure two players are inside
                {
                    if (IsServer)
                    {
                        ShowUiClientRpc(); // Host triggers UI for all clients
                    }
                    else
                    {
                        RequestShowUiServerRpc(); // Client requests the server to trigger UI
                    }
                }
            }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playersInTrigger--; // Decrease count when a player leaves
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestShowUiServerRpc()
    {
        ShowUiClientRpc();
    }

    [ClientRpc]
    private void ShowUiClientRpc()
    {
        CongratsPanel.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCloseUiServerRpc()
    {
        CloseUiClientRpc();
    }

    [ClientRpc]
    private void CloseUiClientRpc()
    {
        CongratsPanel.SetActive(false);
    }
}

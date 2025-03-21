using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BlueButtonActivation : NetworkBehaviour
{
    public GameObject TilemapToMove;
    public Vector3 newPosition;     // The target position for the YellowGround
    private Vector3 initialTilemapPosition;  // Store the original position
    private bool isActivated = false; // Track if the button has been pressed
    Animator animator;
    
    // Network Variable to sync button state
    private NetworkVariable<bool> isButtonPressed = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        if(TilemapToMove != null)
        {
            initialTilemapPosition = TilemapToMove.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (animator != null)
        // {
        //     animator.SetBool("IsPressed", isButtonPressed.Value);
        // }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player steps on the object
        if (collision.gameObject.CompareTag("Player") && !isButtonPressed.Value)
        {
            ActivateButtonServerRpc();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Check if the player steps on the object
        if (collision.gameObject.CompareTag("Player") && isButtonPressed.Value)
        {
            DeactivateButtonServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateButtonServerRpc()
    {
        // if(IsServer)
        // {
            isButtonPressed.Value = true; // Update network state
            MoveYellowGroundClientRpc(); // Call ClientRpc to update all clients
        // }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeactivateButtonServerRpc()
    {
        if (IsServer)
        {
            isButtonPressed.Value = false; // Reset network state
            ResetYellowGroundClientRpc();
        }
    }

    [ClientRpc]
    private void MoveYellowGroundClientRpc()
    {
        if (TilemapToMove != null)
        {
            Debug.Log("moving tilemap");
            StartCoroutine(SmoothMove(TilemapToMove.transform, newPosition, 1f));
        }
        else 
        {
            Debug.LogError("Tilemap reference is missing!");
        }
        animator.SetTrigger("ChangeState");
        Debug.Log("button pressed!");
        
    }

    [ClientRpc]
    private void ResetYellowGroundClientRpc()
    {
        if (TilemapToMove != null)
        {
            Debug.Log("Resetting tilemap");
            StartCoroutine(SmoothMove(TilemapToMove.transform, initialTilemapPosition, 1f));
        }
        animator.Play("ButtonUp"); // Reset animation (change to match your animation state)
        Debug.Log("Button released!");
    }


    // private void MoveYellowGround()
    // {
    //     if (!isActivated && TilemapToMove != null)
    //     {
    //         isActivated = true;
    //         StartCoroutine(SmoothMove(TilemapToMove.transform, newPosition, 1f)); // 1 second duration
    //     }
    //     else
    //     {
    //         Debug.LogError("Tilemap reference is missing!");
    //     }
    // }

    // [ServerRpc(RequireOwnership = false)]
    // public void ResetButtonServerRpc()
    // {
    //     isButtonPressed.Value = false; // Reset state
    //     ResetButtonClientRpc(); // Notify all clients
    // }

    [ClientRpc]
    public void ResetButtonClientRpc()
    {
        isActivated = false; // Allow activation again
        if (TilemapToMove != null)
        {
            StartCoroutine(SmoothMove(TilemapToMove.transform, initialTilemapPosition, 1f)); // Move tilemap back
        }
        animator.Play("ButtonUp"); // Reset animation (change to match your animation state)
    }


    private IEnumerator SmoothMove(Transform tilemapTransform, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = tilemapTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tilemapTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        tilemapTransform.position = targetPosition; // Ensure exact position at the end
    }
}

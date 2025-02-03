using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedButtonActivation : MonoBehaviour
{
    public GameObject TilemapToMove;
    public Vector3 newPosition;     // The target position for the YellowGround
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player steps on the object
        if (collision.gameObject.CompareTag("Player"))
        {
            // Trigger the state change animation
            animator.SetTrigger("ChangeState");
            MoveYellowGround();
        }
    }

     private void MoveYellowGround()
    {
        if (TilemapToMove != null)
        {
            StartCoroutine(SmoothMove(TilemapToMove.transform, newPosition, 1f)); // 1 second duration
        }
        else
        {
            Debug.LogError("Tilemap reference is missing!");
        }
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

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using Unity.Netcode;

// public class NewPlayerMovement : NetworkBehaviour
// {

//     public Rigidbody2D rb;
//     public float speed;
//     private float Move;
//     public float jump;
//     public bool isJumping;
    
//     private SpriteRenderer spriteRenderer;
//     Animator animator;

    
//     // Start is called before the first frame update
//     void Start()
//     {
//         spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer
//         animator = GetComponent<Animator>();
//     }

//     // public override void OnNetworkSpawn()
//     // {
//     //     // spriteRenderer = GetComponent<SpriteRenderer>();
//     //     // animator = GetComponent<Animator>();
//     //     if (IsOwner) 
//     //     {
//     //         enabled = false;
//     //         return;
//     //     }
//     // }


//     // Update is called once per frame
//     void Update()
//     {
//         if (!IsOwner) return; // Only the owner can control movement

// // uncomment from here
//         Move = Input.GetAxis("Horizontal");
//         // Send movement input to the server
//         MoveServerRpc(Move);

//         rb.velocity = new Vector2( Move * speed, rb.velocity.y);
//         if(Input.GetKeyDown(KeyCode.LeftShift)) {
//             animator.Play("WalkingAnim");
//         }

//         // Flip the sprite based on movement direction
//         if (Move > 0) // Moving right
//         {
//             spriteRenderer.flipX = false;
//             animator.SetBool("isWalking", true);
//             // animator.Play("WalkingState");
//         }
//         else if (Move < 0) // Moving left
//         {
//             spriteRenderer.flipX = true;
//             animator.SetBool("isWalking", true);
//         }

//         else {
//             animator.SetBool("isWalking", false);
//         }

        
//         // if (Input.GetButtonDown("Jump") && isJumping == false) 
//         // {
//         //     rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
//         // }

//         // Handle Jumping
//         if (Input.GetButtonDown("Jump") && !isJumping)
//         {
//             JumpServerRpc();
//         }

//     }

//     [ServerRpc]
//     void MoveServerRpc(float move)
//     {
//         rb.velocity = new Vector2(move * speed, rb.velocity.y);

//         // Sync animation and sprite flipping across clients
//         FlipClientRpc(move);
//     }

//     [ServerRpc]
//     void JumpServerRpc()
//     {
//         if (!isJumping)
//         {
//             rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
//             isJumping = true;

//             // Notify all clients that a jump happened
//             JumpClientRpc();
//         }
//     }

//     [ClientRpc]
//     void FlipClientRpc(float move)
//     {
//         // if(!IsOwner) return; //Prevent running on non-owners
//         if (spriteRenderer == null || animator == null) return; // Prevent null error

//         if (move > 0)
//         {
//             spriteRenderer.flipX = false;
//             animator.SetBool("isWalking", true);
//         }
//         else if (move < 0)
//         {
//             spriteRenderer.flipX = true;
//             animator.SetBool("isWalking", true);
//         }
//         else
//         {
//             animator.SetBool("isWalking", false);
//         }
//     }

//     [ClientRpc]
//     void JumpClientRpc()
//     {
//         if (!IsOwner) return; // Prevent running on non-owners

//         animator.SetTrigger("Jump"); // Play jump animation
//     }

//     private void OnCollisionEnter2D(Collision2D other) {
//         // if player is on the ground, means not jumping
//         if (other.gameObject.CompareTag("Ground")) 
//         {
//             isJumping = false;
//         }
//     }
//     private void OnCollisionExit2D(Collision2D other) {
//         // if player is NOT on the ground, means jumping
//         if (other.gameObject.CompareTag("Ground")) 
//         {
//             isJumping = true;
//         }
//     }


// // Uncomment here
//     //     Move = Input.GetAxis("Horizontal");
//     //     rb.velocity = new Vector2( Move * speed, rb.velocity.y);
//     //     if(Input.GetKeyDown(KeyCode.LeftShift)) {
//     //         animator.Play("WalkingAnim");
//     //     }

//     //     // Flip the sprite based on movement direction
//     //     if (Move > 0) // Moving right
//     //     {
//     //         spriteRenderer.flipX = false;
//     //         animator.SetBool("isWalking", true);
//     //         // animator.Play("WalkingState");
//     //     }
//     //     else if (Move < 0) // Moving left
//     //     {
//     //         spriteRenderer.flipX = true;
//     //         animator.SetBool("isWalking", true);
//     //     }

//     //     else {
//     //         animator.SetBool("isWalking", false);
//     //     }
        
//     //     if (Input.GetButtonDown("Jump") && isJumping == false) 
//     //     {
//     //         rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
//     //     }

//     // }

//     // private void OnCollisionEnter2D(Collision2D other) {
//     //     // if player is on the ground, means not jumping
//     //     if (other.gameObject.CompareTag("Ground")) 
//     //     {
//     //         isJumping = false;
//     //     }
//     // }
//     // private void OnCollisionExit2D(Collision2D other) {
//     //     // if player is NOT on the ground, means jumping
//     //     if (other.gameObject.CompareTag("Ground")) 
//     //     {
//     //         isJumping = true;
//     //     }
//     // }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NewPlayerMovement : NetworkBehaviour
{
    public Rigidbody2D rb;
    public float speed;
    private float Move;
    public float jumpForce;
    
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private NetworkVariable<bool> isJumping = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return; // Only the owner should process inputs
        
        Move = Input.GetAxis("Horizontal");

        // Send movement to the server
        MoveServerRpc(Move);

        if (Input.GetButtonDown("Jump") && !isJumping.Value)
        {
            JumpServerRpc();
        }
    }

    [ServerRpc]
    void MoveServerRpc(float move)
    {
        rb.velocity = new Vector2(move * speed, rb.velocity.y);
        FlipClientRpc(move);
    }

    [ServerRpc(RequireOwnership = false)]
    void JumpServerRpc()
    {
        if (!isJumping.Value)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping.Value = true;  // Mark as jumping so it doesn’t double jump
        }
    }

    [ClientRpc]
    void FlipClientRpc(float move)
    {
        if (spriteRenderer == null) return;
        
        if (move > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (move < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isJumping.Value = false; // Reset jump state when touching the ground
        }
    }
}

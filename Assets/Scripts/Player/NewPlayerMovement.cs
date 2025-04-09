using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Cinemachine;
using Unity.Services.Lobbies.Models;

public class NewPlayerMovement : NetworkBehaviour
{

    public Rigidbody2D rb;
    public float speed;
    private float Move;
    public float jump;
    // public bool isJumping;
    public AudioClip jumpSound;

    private SpriteRenderer spriteRenderer;
    Animator animator;
    private CinemachineVirtualCamera cinemachineCam;
    private AudioSource audioSource;

    // Use NetworkVariable to sync isJumping
    private NetworkVariable<bool> isJumping = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer
        animator = GetComponent<Animator>();

        if (IsOwner) // Only apply the camera to the player that owns this object
        {
            cinemachineCam = FindObjectOfType<CinemachineVirtualCamera>();
            if (cinemachineCam != null)
            {
                cinemachineCam.Follow = transform;  // Make the camera follow this player
            }
            else
            {
                Debug.LogError("Cinemachine Virtual Camera not found in the scene!");
            }
        }
    }

    // public override void OnNetworkSpawn()
    // {
    //     // spriteRenderer = GetComponent<SpriteRenderer>();
    //     // animator = GetComponent<Animator>();
    //     if (IsOwner) 
    //     {
    //         enabled = false;
    //         return;
    //     }
    // }


    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return; // Only the owner can control movement

        // uncomment from here
        Move = Input.GetAxis("Horizontal");
        // Send movement input to the server
        MoveServerRpc(Move);

        rb.velocity = new Vector2(Move * speed, rb.velocity.y);
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.Play("WalkingAnim");
        }

        // Flip the sprite based on movement direction
        if (Move > 0) // Moving right
        {
            spriteRenderer.flipX = false;
            animator.SetBool("isWalking", true);
            // animator.Play("WalkingState");
        }
        else if (Move < 0) // Moving left
        {
            spriteRenderer.flipX = true;
            animator.SetBool("isWalking", true);
        }

        else
        {
            animator.SetBool("isWalking", false);
        }


        // if (Input.GetButtonDown("Jump") && isJumping == false) 
        // {
        //     rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
        // }

        // Handle Jumping
        if (Input.GetButtonDown("Jump") && !isJumping.Value)
        {
            JumpServerRpc();
        }

    }

    [ServerRpc]
    void MoveServerRpc(float move)
    {
        rb.velocity = new Vector2(move * speed, rb.velocity.y);

        // Sync animation and sprite flipping across clients
        FlipClientRpc(move);
    }

    [ServerRpc]
    void JumpServerRpc()
    {
        if (!isJumping.Value)
        {
            // rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
            isJumping.Value = true;

            // Notify all clients that a jump happened
            JumpClientRpc();
        }
    }

    [ClientRpc]
    void FlipClientRpc(float move)
    {
        // if(!IsOwner) return; //Prevent running on non-owners
        if (spriteRenderer == null || animator == null) return; // Prevent null error

        if (move > 0)
        {
            spriteRenderer.flipX = false;
            animator.SetBool("isWalking", true);
        }
        else if (move < 0)
        {
            spriteRenderer.flipX = true;
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    [ClientRpc]
    void JumpClientRpc()
    {
        if (!IsOwner) return; // Prevent running on non-owners

        rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse); // Apply force locally
        audioSource.PlayOneShot(jumpSound);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // if player is on the ground, means not jumping
        if (other.gameObject.CompareTag("Ground"))
        {
            RequestSetJumpingServerRpc(false);
            // isJumping.Value = false;
        }
    }
    private void OnCollisionExit2D(Collision2D other)
    {
        // if player is NOT on the ground, means jumping
        if (other.gameObject.CompareTag("Ground"))
        {
            RequestSetJumpingServerRpc(true);
            // isJumping.Value = true;
        }
    }

    [ServerRpc]
    void RequestSetJumpingServerRpc(bool value)
    {
        isJumping.Value = value;
    }

    // Uncomment here
    //     Move = Input.GetAxis("Horizontal");
    //     rb.velocity = new Vector2( Move * speed, rb.velocity.y);
    //     if(Input.GetKeyDown(KeyCode.LeftShift)) {
    //         animator.Play("WalkingAnim");
    //     }

    //     // Flip the sprite based on movement direction
    //     if (Move > 0) // Moving right
    //     {
    //         spriteRenderer.flipX = false;
    //         animator.SetBool("isWalking", true);
    //         // animator.Play("WalkingState");
    //     }
    //     else if (Move < 0) // Moving left
    //     {
    //         spriteRenderer.flipX = true;
    //         animator.SetBool("isWalking", true);
    //     }

    //     else {
    //         animator.SetBool("isWalking", false);
    //     }

    //     if (Input.GetButtonDown("Jump") && isJumping == false) 
    //     {
    //         rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
    //     }

    // }

    // private void OnCollisionEnter2D(Collision2D other) {
    //     // if player is on the ground, means not jumping
    //     if (other.gameObject.CompareTag("Ground")) 
    //     {
    //         isJumping = false;
    //     }
    // }
    // private void OnCollisionExit2D(Collision2D other) {
    //     // if player is NOT on the ground, means jumping
    //     if (other.gameObject.CompareTag("Ground")) 
    //     {
    //         isJumping = true;
    //     }
    // }
}

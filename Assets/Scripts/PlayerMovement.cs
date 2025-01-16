using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    // private float horizontal;
    
    // private float jumpingPower = 16f;
    private bool isFacingRight = true;
    public Rigidbody2D rb;
    public float speed;
    private float Move;
    public float jump;
    public bool isJumping;

   
//    [SerializeField] private Transform groundCheck;
//    [SerializeField] private LayerMask groundLayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        Move = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2( Move * speed, rb.velocity.y);
        Flip();
        
        if (Input.GetButtonDown("Jump") && isJumping == false) 
        {
            rb.AddForce(new Vector2(rb.velocity.x, jump));
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        // if player is on the ground, means not jumping
        if (other.gameObject.CompareTag("Ground")) 
        {
            isJumping = false;
        }
    }
    private void OnCollisionExit2D(Collision2D other) {
        // if player is NOT on the ground, means jumping
        if (other.gameObject.CompareTag("Ground")) 
        {
            isJumping = true;
        }
    }


    private void Flip() {
        if (isFacingRight && Move < 0f || !isFacingRight && Move > 0f) 
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= 1f;
            transform.localScale = localScale;
        }
    }
}

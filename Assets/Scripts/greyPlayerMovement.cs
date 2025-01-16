using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class greyPlayerMovement : MonoBehaviour
{

    public Rigidbody2D rb;
    public float speed;
    private float Move;
    public float jump;
    public bool isJumping;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2( Move * speed, rb.velocity.y);

        //Ensures no player movement once arrow keys are not held down (doesnt work)
        // if (Move != 0)
        // {
        //     rb.velocity = new Vector2(Move * speed, rb.velocity.y); // Apply horizontal velocity
        // }
        // else
        // {
        //     rb.velocity = new Vector2(0, rb.velocity.y); // Stop horizontal movement
        // }
        
        if (Input.GetButtonDown("Jump") && isJumping == false) 
        {
            rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
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
}

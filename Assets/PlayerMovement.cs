using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class PlayerMovement : MonoBehaviour
{
    // private float horizontal;
    
    // private float jumpingPower = 16f;
    private bool isFacingRight = true;
    public Rigidbody2D rb;
    public float speed;
    private float Move;

   
//    [SerializeField] private Transform groundCheck;
//    [SerializeField] private LayerMask groundLayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2( Move * speed, rb.velocity.y);
        Flip();
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

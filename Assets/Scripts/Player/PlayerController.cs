using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!IsOwner) return; // Only allow input for the player who owns this object

        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            rb.velocity = moveInput * speed;
        }
    }
}

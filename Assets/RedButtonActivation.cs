using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedButtonActivation : MonoBehaviour
{
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
        }
    }
}

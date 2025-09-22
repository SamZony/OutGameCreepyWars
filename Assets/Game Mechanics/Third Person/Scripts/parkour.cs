using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parkour : MonoBehaviour
{
    private Animator animator;
    public float groundCheckDistance = 5f; // Adjust this value for your needs
    private Rigidbody rb;

    public float jumpHeight = 4f;
    public float jumpDuration = 0.5f;

    public float maxClimbHeight = 4f; // Adjust the maximum climb height as needed
    public float detectionDistance = 1f; // Adjust the detection distance as needed
    private Vector3 positionPostClimb;

    private bool isJumping = false;
    private float jumpStartTime;

    public List<MonoBehaviour> scriptsToDisableOnCollision = new();
    public List<CapsuleCollider> capsuleColliders = new();

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        RaycastHit hit;
        // Check for ground below the playerActionMap
        if (Physics.Raycast(transform.position, Vector3.up, out hit, groundCheckDistance))
        {
            // Ground found
            animator.SetBool("isFalling", false);
        }
        else
        {
            // No ground found
            animator.SetBool("isFalling", true);
        }

        //JUMPING Mechanics --------------
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            Jump();
        }

        if (isJumping)
        {
            // Calculate elapsed time since jump started
            float elapsedTime = Time.time - jumpStartTime;

            // Calculate jump height based on elapsed time and jump duration
            float jumpProgress = elapsedTime / jumpDuration;
            float currentHeight = jumpHeight * jumpProgress * (2 - jumpProgress);

            // Apply jump elevation
            transform.position += currentHeight * Time.deltaTime * Vector3.up;

            // Check if jump duration has elapsed
            if (elapsedTime >= jumpDuration)
            {
                isJumping = false;
            }
        }

        //CLIMBING the objects under 4 ft
        if (Physics.Raycast(transform.Find("lookAt").position, transform.forward, out hit, detectionDistance))
        {
            // Check if the object is within the maximum climb height
            if (hit.distance <= 1)
            {
                // Check if space is pressed
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine(climbingWall());
                    positionPostClimb = transform.position + transform.forward * hit.distance + new Vector3(0, 0.3f, 0) * maxClimbHeight;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BottomCollider"))
        {
            // Disable the playerActionMap's animator
            animator.enabled = false;

            // Disable scripts in the list
            foreach (MonoBehaviour script in scriptsToDisableOnCollision)
            {
                script.enabled = false;
            }
            foreach (CapsuleCollider cap in capsuleColliders)
            {
                cap.enabled = false;
            }
        }
    }

    IEnumerator climbingWall()
    {

        // Play the "climb" state from the animator
        animator.Play("wall_climb");
        yield return new WaitForSeconds(1);
        transform.position = positionPostClimb;
    }
    void Jump()
    {
        float verticalAxis = Input.GetAxis("Vertical");
        if (verticalAxis > 0f)
        {
            isJumping = true;
            jumpStartTime = Time.time;

            animator.Play("jump_forward");

            // Add initial upward force (optional)
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);

        }
    }
}

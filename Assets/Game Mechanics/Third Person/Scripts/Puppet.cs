using System.Collections.Generic;
using UnityEngine;

public class Puppet : MonoBehaviour
{
    public Rigidbody mainRigidbody;
    public Animator animator;
    public Transform rootBone; // Root bone of the ragdoll hierarchy
    public float movementForce = 50f; // Force applied to move the character
    public float wobbleAmount = 5f; // Amount of wobble applied
    private List<Transform> ragdollBones; // List of ragdoll bones to apply forces

    void Start()
    {
        ragdollBones = new List<Transform>();
        GatherBones(rootBone, ragdollBones);

        // Ensure each ragdoll bone has a Rigidbody
        foreach (Transform bone in ragdollBones)
        {
            if (bone.GetComponent<Rigidbody>() == null)
            {
                bone.gameObject.AddComponent<Rigidbody>();
            }
        }
    }

    void Update()
    {
        // Get input for movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movementDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Apply movement force to the main Rigidbody
        mainRigidbody.AddForce(movementDirection * movementForce);

        // Apply wobble to each ragdoll bone
        foreach (Transform bone in ragdollBones)
        {
            Rigidbody rb = bone.GetComponent<Rigidbody>();
            Vector3 wobbleForce = new Vector3(
                Random.Range(-wobbleAmount, wobbleAmount),
                Random.Range(-wobbleAmount, wobbleAmount),
                Random.Range(-wobbleAmount, wobbleAmount)
            );
            rb.AddForce(wobbleForce);
        }
    }

    void GatherBones(Transform root, List<Transform> bones)
    {
        foreach (Transform child in root)
        {
            bones.Add(child);
            GatherBones(child, bones);
        }
    }
}

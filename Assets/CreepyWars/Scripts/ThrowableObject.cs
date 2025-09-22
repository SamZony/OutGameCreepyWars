using System;
using UnityEngine;

public class ThrowableObject : MonoBehaviour
{
    Rigidbody rb;
    Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OutGameManager.Instance.currentPickableObject = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OutGameManager.Instance.currentPickableObject = null;
        }
    }
}

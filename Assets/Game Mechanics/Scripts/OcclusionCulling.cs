using UnityEngine;
using System.Collections.Generic;

public class OcclusionCulling : MonoBehaviour
{
    public LayerMask obstacleLayer;
    public float checkDistance = 10f;
    public List<GameObject> exceptionObjects; // List of exception objects

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (exceptionObjects.Contains(gameObject))
        {
            gameObject.SetActive(true); // Object is in the exception list
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, transform.position - mainCamera.transform.position, out hit, checkDistance, obstacleLayer))
        {
            gameObject.SetActive(false); // Object is behind an obstacle
        }
        else
        {
            gameObject.SetActive(true); // Object is visible
        }
    }
}

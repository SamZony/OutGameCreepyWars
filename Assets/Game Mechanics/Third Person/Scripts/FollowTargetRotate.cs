
using UnityEngine;

public class FollowTargetRotate : MonoBehaviour
{
    public float rotationSpeed = 10f;

    private Vector2 mouseLook;

    private void Update()
    {
        mouseLook.x += Input.GetAxis("Mouse X");
        mouseLook.y -= Input.GetAxis("Mouse Y");

        // Limit the vertical rotation to prevent inversion
        mouseLook.y = Mathf.Clamp(mouseLook.y, -90f, 90f);

        // Rotate the game object itself if needed
        transform.rotation = Quaternion.Euler(mouseLook.y * rotationSpeed, mouseLook.x * rotationSpeed, 0f);
    }
}

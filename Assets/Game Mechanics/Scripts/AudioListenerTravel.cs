using UnityEngine;

public class AudioListenerTravel : MonoBehaviour
{
    public AudioListener audioListener;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioListener = GetComponent<AudioListener>();
        if (audioListener == null)
        {
            audioListener = gameObject.AddComponent<AudioListener>();

        }
    }

    // Update is called once per frame
    void Update()
    {
        Camera[] cameras = Camera.allCameras;
        Camera activeCamera = null;

        // Prioritize cameras with culling mask including "Default" layer
        foreach (Camera camera in cameras)
        {
            if (camera.cullingMask == 0) // Check if the camera is rendering the "Default" layer
            {
                activeCamera = camera;
                break;
            }
        }

        // If no camera with "Default" layer found, use the first enabled camera
        if (activeCamera == null)
        {
            foreach (Camera camera in cameras)
            {
                if (camera.enabled)
                {
                    activeCamera = camera;
                    break;
                }
            }
        }

        if (activeCamera != null && audioListener.gameObject != activeCamera.gameObject)
        {
            audioListener.transform.position = activeCamera.transform.position;
        }
    }
}

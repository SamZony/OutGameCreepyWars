using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class DayAndNight : MonoBehaviour
{
    public GameObject Sun;
    public float speed = 2;
    public Quaternion SunRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Sun == null)
        {
            Debug.Log("Assign Directional Light Game Object");
        }
    }

    // Update is called once per frame
    void Update()
    {
        float angleChange = speed * Time.deltaTime;

        // Create a Quaternion representing the incremental rotation
        Quaternion deltaRotation = Quaternion.Euler(0, angleChange, 0);

        // Apply the incremental rotation to the current rotation
        Sun.transform.rotation *= deltaRotation;

        // Update SunRotation for reference (optional)
        SunRotation = Sun.transform.rotation;
    }
}

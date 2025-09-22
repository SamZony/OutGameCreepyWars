using UnityEngine;

public class HelicopterEngineInitially : MonoBehaviour
{
    private GameObject HelicopterModel;
    private MonoBehaviour HelicopterController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HelicopterController = HelicopterModel.GetComponent<HelicopterController>();
        if (HelicopterController == null)
        {
            Debug.LogError("Assign the Helicopter Controller in your scene into Helicopter Engine script");
        }
        else
        {

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

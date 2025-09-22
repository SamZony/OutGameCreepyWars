using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class MoveVClouds : MonoBehaviour
{
    public Volume volume;
    private VolumetricClouds clouds;
    private WindSpeedParameter.WindParamaterValue windSpeed;


    private void Start()
    {
        volume  = GetComponent<Volume>();
        if (volume.profile.TryGet<VolumetricClouds>(out clouds))
        {
            Debug.Log("Found the clouds");
        }

    }

    private void Update()
    {
        
    }
}

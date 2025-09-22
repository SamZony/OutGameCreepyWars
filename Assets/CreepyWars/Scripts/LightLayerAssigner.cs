using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class LightLayerAssigner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UnityEngine.Rendering.HighDefinition.RenderingLayerMask lightLayerMask;

    void Start()
    {
        GetComponent<HDAdditionalLightData>().lightlayersMask = lightLayerMask;
    }
}

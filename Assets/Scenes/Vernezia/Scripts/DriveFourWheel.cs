using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;


public class DriveFourWheel : MonoBehaviour
{
    public Rigidbody vehicleRigidbody;
    public float gasForce;
    public bool goReverse = true;
    public bool goForward = false;

    [Header("Lights")]
    public List<Light> backLights;
    public List<Light> frontLights;
    public float maxIntensityBack;
    public float maxIntensityFront;

    void Start()
    {
        for (int i = 0; i < backLights.Count; i++)
        {
            backLights[i].intensity = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float vehicleAxis = Input.GetAxis("Vertical");
        Vector3 moveForce = vehicleAxis * Time.deltaTime * gasForce * transform.forward;


        if (vehicleAxis >= 0.5 && goForward)
        {
            vehicleRigidbody.AddForce(moveForce, ForceMode.Acceleration);
        }
        if (vehicleAxis <= -0.5 && goReverse)
        {
            vehicleRigidbody.AddForce(moveForce, ForceMode.Acceleration);

            for (int i = 0; i < backLights.Count; i++)
            {
                backLights[i].intensity = maxIntensityBack;
            }
        }
        else
        {
            for (int i = 0; i < backLights.Count; i++)
            {
                backLights[i].intensity = 0;
            }
        }


        //-------------------------------------------------------------------------------------------

        if (Input.GetKeyDown(KeyCode.H))
        {

            for (int i = 0; i < frontLights.Count; i++)
            {
                if (frontLights[i].intensity == 0)
                {
                    frontLights[i].intensity = maxIntensityFront;
                }
                else
                {
                    frontLights[i].intensity = 0;
                }

            }

        }

    }
}

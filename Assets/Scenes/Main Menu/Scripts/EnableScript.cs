using UnityEngine;
using System.Collections;
using UnityEngine.Sequences;

public class EnableScript : MonoBehaviour
{
    public MonoBehaviour Script;
    public bool Should;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Should)
        {
            Script.enabled = true;
        }
        else
        {
            Script.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

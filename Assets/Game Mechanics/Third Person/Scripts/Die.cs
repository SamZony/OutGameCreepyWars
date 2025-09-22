using UnityEngine;

public class die : MonoBehaviour
{
    public Animator PlayerAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            GameObject.FindWithTag("Player").transform.Find("weaponSound").GetComponent<AudioSource>().Play();
            gameObject.GetComponent<Animator>().enabled = false;
        }
    }
}

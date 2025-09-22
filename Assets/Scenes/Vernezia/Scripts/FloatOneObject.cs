using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Buoyancy : MonoBehaviour
{
    public Rigidbody rb;
    public float depthBefSub;
    public float displacementAmt;
    public int floaters;

    public LayerMask groundLayer;
    public float heightOffset;

    public float waterDrag;
    public float waterAngularDrag;
    public WaterSurface water;
    WaterSearchParameters Search;
    WaterSearchResult SearchResult;

    void FixedUpdate()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * heightOffset;
        Ray ray = new Ray(rayOrigin, Vector3.down);

        // Cast the ray
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            rb.AddForceAtPosition(Physics.gravity / floaters, transform.position, ForceMode.Acceleration);

            Search.startPositionWS = transform.position;

            water.ProjectPointOnWaterSurface(Search, out SearchResult);

            if (transform.position.y < SearchResult.projectedPositionWS.y)
            {

                float displacementMulti = Mathf.Clamp01(SearchResult.projectedPositionWS.y - transform.position.y / depthBefSub) * displacementAmt;
                rb.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMulti, 0f), transform.position, ForceMode.Acceleration);
                rb.AddForce(displacementMulti * -rb.linearVelocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                rb.AddTorque(displacementMulti * Time.fixedDeltaTime * waterAngularDrag * -rb.angularVelocity, ForceMode.VelocityChange);
            }
        }

    }
}
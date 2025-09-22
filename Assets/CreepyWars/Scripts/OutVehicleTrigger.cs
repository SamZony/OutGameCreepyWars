using System.Runtime.Serialization;
using UnityEngine;

public class OutVehicleTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        EnterTrigger,
        ExitTrigger
    }

    public TriggerType triggerType;

    [Tooltip("Vehicle this trigger is for")]
    public OutVehicleController thisVehicle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (triggerType == TriggerType.EnterTrigger)
            {
                OutGameManager.Instance.currentVehicle = thisVehicle;
            }
            else
            {
                OutGameManager.Instance.currentVehicle = null;
            }

        }
    }
}

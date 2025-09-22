using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class RespawnPlayer: MonoBehaviour
{
    public void RestartFromCheckpoint()
    {
        Transform checkpoint = CheckpointManager.Instance.GetLastCheckpoint();

        if (checkpoint != null)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc) cc.enabled = false; // avoid teleport glitches

            Rigidbody rb = transform.GetComponent<Rigidbody>();
            rb.position = checkpoint.position;
            rb.rotation = checkpoint.rotation;

            if (cc) cc.enabled = true;

            GetComponent<OutShooterController>().Health = GetComponent<OutShooterController>().maxHealth;
            EnableEverything(gameObject);
        }
        else
        {
            Debug.LogWarning("No checkpoint found. Respawn failed.");
        }
    }

    public void EnableEverything(GameObject obj)
    {
        // Enable the GameObject itself in case it's inactive
        obj.SetActive(true);

        // Enable all components that have an 'enabled' property
        foreach (var component in obj.GetComponentsInChildren<Component>(true))
        {
            var type = component.GetType();
            var enabledProp = type.GetProperty("enabled");

            if (enabledProp != null && enabledProp.PropertyType == typeof(bool))
            {
                enabledProp.SetValue(component, true, null);
            }
        }

        // Set Ragdoll State, 
        GetComponent<OutTPSystem>().SetRagdollState(true); // True means animator is true so that the ragdoll has to be disabled
    }
}

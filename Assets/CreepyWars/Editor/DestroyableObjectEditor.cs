using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DestroyableObject))]
public class DestroyableObjectEditor : Editor
{
    SerializedProperty destroyMethod;
    SerializedProperty destructionResult;

    SerializedProperty minAttackerMass;
    SerializedProperty minAttackerVelocity;
    SerializedProperty minAttackTimes;
    SerializedProperty fallOffSound;

    SerializedProperty explosionForce;
    SerializedProperty upwardsModifier;
    SerializedProperty fireParticlePrefab;
    SerializedProperty explosionParticlePrefab;
    SerializedProperty explosionSound;
    SerializedProperty maxFireSpawns;

    SerializedProperty afterBreaking;

    void OnEnable()
    {
        destroyMethod = serializedObject.FindProperty("destroyMethod");
        destructionResult = serializedObject.FindProperty("destructionResult");

        minAttackerMass = serializedObject.FindProperty("minAttackerMass");
        minAttackerVelocity = serializedObject.FindProperty("minAttackerVelocity");
        minAttackTimes = serializedObject.FindProperty("minAttackTimes");
        fallOffSound = serializedObject.FindProperty("fallOffSound");

        explosionForce = serializedObject.FindProperty("explosionForce");
        upwardsModifier = serializedObject.FindProperty("upwardsModifier");
        fireParticlePrefab = serializedObject.FindProperty("fireParticlePrefab");
        explosionParticlePrefab = serializedObject.FindProperty("explosionParticlePrefab");
        explosionSound = serializedObject.FindProperty("explosionSound");
        maxFireSpawns = serializedObject.FindProperty("maxFireSpawns");

        afterBreaking = serializedObject.FindProperty("afterBreaking");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(destroyMethod);

        // Show "attacker requirements" only for Collision or Trigger
        if (destroyMethod.enumValueIndex == (int)DestroyableObject.DestroyMethod.Collision ||
            destroyMethod.enumValueIndex == (int)DestroyableObject.DestroyMethod.Trigger)
        {
            EditorGUILayout.PropertyField(minAttackerMass);
            EditorGUILayout.PropertyField(minAttackerVelocity);
            EditorGUILayout.PropertyField(minAttackTimes);
        }

        EditorGUILayout.PropertyField(destructionResult);

        // FallOff fields
        if (destructionResult.enumValueIndex == (int)DestroyableObject.DestructionResult.FallOff)
        {
            EditorGUILayout.PropertyField(fallOffSound);
        }

        // Explosion fields
        if (destructionResult.enumValueIndex == (int)DestroyableObject.DestructionResult.Explode)
        {
            EditorGUILayout.PropertyField(explosionForce);
            EditorGUILayout.PropertyField(upwardsModifier);
            EditorGUILayout.PropertyField(fireParticlePrefab);
            EditorGUILayout.PropertyField(explosionParticlePrefab);
            EditorGUILayout.PropertyField(explosionSound);
            EditorGUILayout.PropertyField(maxFireSpawns);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionDelay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("persistExplosionAfterExit"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(afterBreaking);

        serializedObject.ApplyModifiedProperties();
    }
}

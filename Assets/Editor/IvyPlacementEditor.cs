using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GrowthManager))]
public class IvyPlacementEditor : Editor
{
    private GrowthManager growthManager;
    private SerializedProperty placementMode;
    private SerializedProperty rootPlaceholders;
    private SerializedProperty orientTowardCenter;
    private SerializedProperty keepPlaceholders;
    
    // Automatic mode properties
    private SerializedProperty spawnRadius;
    private SerializedProperty rootCount;
    private SerializedProperty minSpawnDistance;
    private SerializedProperty maxSpawnAttempts;
    private SerializedProperty positionJitter;
    
    private void OnEnable()
    {
        growthManager = (GrowthManager)target;
        
        // Get serialized properties
        placementMode = serializedObject.FindProperty("placementMode");
        rootPlaceholders = serializedObject.FindProperty("rootPlaceholders");
        orientTowardCenter = serializedObject.FindProperty("orientTowardCenter");
        keepPlaceholders = serializedObject.FindProperty("keepPlaceholders");
        
        // Get automatic mode properties
        spawnRadius = serializedObject.FindProperty("spawnRadius");
        rootCount = serializedObject.FindProperty("rootCount");
        minSpawnDistance = serializedObject.FindProperty("minSpawnDistance");
        maxSpawnAttempts = serializedObject.FindProperty("maxSpawnAttempts");
        positionJitter = serializedObject.FindProperty("positionJitter");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Draw default inspector for properties not explicitly handled
        DrawPropertiesExcluding(serializedObject, 
            "placementMode", "rootPlaceholders", "orientTowardCenter", "keepPlaceholders",
            "spawnRadius", "rootCount", "minSpawnDistance", "maxSpawnAttempts", "positionJitter");
        
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(placementMode);
        
        // Draw properties based on placement mode
        GrowthManager.PlacementMode mode = (GrowthManager.PlacementMode)placementMode.enumValueIndex;
        
        if (mode == GrowthManager.PlacementMode.Automatic)
        {
            EditorGUILayout.LabelField("Automatic Placement Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(spawnRadius);
            EditorGUILayout.PropertyField(rootCount);
            EditorGUILayout.PropertyField(minSpawnDistance);
            EditorGUILayout.PropertyField(maxSpawnAttempts);
            EditorGUILayout.PropertyField(positionJitter);
        }
        else // Manual mode
        {
            EditorGUILayout.LabelField("Manual Placement Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(orientTowardCenter);
            EditorGUILayout.PropertyField(keepPlaceholders);
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(rootPlaceholders);
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Add New Placeholder"))
            {
                CreatePlaceholder();
            }
            
            if (GUILayout.Button("Select All Placeholders"))
            {
                SelectAllPlaceholders();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Arrange in Circle"))
            {
                ArrangePlaceholdersInCircle();
            }
            
            if (GUILayout.Button("Orient All Toward Center"))
            {
                OrientAllPlaceholdersTowardCenter();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Display helpful usage instructions
            EditorGUILayout.HelpBox(
                "1. Add placeholders using 'Add New Placeholder' button\n" +
                "2. Position each placeholder where you want ivy to grow\n" +
                "3. Rotate placeholders to control growth direction (if not oriented toward center)\n" +
                "4. Use the tools to quickly arrange placeholders", 
                MessageType.Info);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void CreatePlaceholder()
    {
        // Create a new placeholder at the growth manager's position
        GameObject placeholder = new GameObject("IvyRootPlaceholder");
        Undo.RegisterCreatedObjectUndo(placeholder, "Create Ivy Root Placeholder");
        
        // Position the placeholder at a distance from the center
        placeholder.transform.position = growthManager.transform.position + Vector3.right * growthManager.spawnRadius;
        
        // Orient toward center by default
        placeholder.transform.up = (growthManager.transform.position - placeholder.transform.position).normalized;
        
        // Add to the list of placeholders
        int index = rootPlaceholders.arraySize;
        rootPlaceholders.arraySize++;
        rootPlaceholders.GetArrayElementAtIndex(index).objectReferenceValue = placeholder.transform;
        
        // Select the new placeholder
        Selection.activeGameObject = placeholder;
    }
    
    private void SelectAllPlaceholders()
    {
        List<Object> objectsToSelect = new List<Object>();
        
        for (int i = 0; i < rootPlaceholders.arraySize; i++)
        {
            Transform placeholder = rootPlaceholders.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
            if (placeholder != null)
            {
                objectsToSelect.Add(placeholder.gameObject);
            }
        }
        
        if (objectsToSelect.Count > 0)
        {
            Selection.objects = objectsToSelect.ToArray();
        }
    }
    
    private void ArrangePlaceholdersInCircle()
    {
        int count = rootPlaceholders.arraySize;
        if (count == 0) return;
        
        float radius = growthManager.spawnRadius;
        Vector3 center = growthManager.transform.position;
        
        for (int i = 0; i < count; i++)
        {
            Transform placeholder = rootPlaceholders.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
            if (placeholder != null)
            {
                // Calculate position around circle
                float angle = i * (360f / count);
                Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
                Vector3 position = center + direction * radius;
                
                Undo.RecordObject(placeholder, "Arrange Placeholders in Circle");
                placeholder.position = position;
                
                // Orient toward center if specified
                if (orientTowardCenter.boolValue)
                {
                    placeholder.up = (center - position).normalized;
                }
            }
        }
    }
    
    private void OrientAllPlaceholdersTowardCenter()
    {
        Vector3 center = growthManager.transform.position;
        
        for (int i = 0; i < rootPlaceholders.arraySize; i++)
        {
            Transform placeholder = rootPlaceholders.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
            if (placeholder != null)
            {
                Undo.RecordObject(placeholder, "Orient Toward Center");
                placeholder.up = (center - placeholder.position).normalized;
            }
        }
    }
    
    // Draw additional handles in scene view
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    private static void DrawPlaceholderGizmos(GrowthManager growthManager, GizmoType gizmoType)
    {
        if (growthManager.placementMode != GrowthManager.PlacementMode.Manual)
            return;
            
        foreach (Transform placeholder in growthManager.rootPlaceholders)
        {
            if (placeholder == null) continue;
            
            // Draw a visual representation of each placeholder
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(placeholder.position, 0.2f);
            
            // Draw arrow showing growth direction
            Vector3 direction;
            if (growthManager.orientTowardCenter)
            {
                direction = (growthManager.transform.position - placeholder.position).normalized;
            }
            else
            {
                direction = placeholder.up;
            }
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(placeholder.position, direction * 1.0f);
        }
    }
} 
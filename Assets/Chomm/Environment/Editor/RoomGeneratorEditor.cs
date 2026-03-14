using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomGenerator))]
[CanEditMultipleObjects]
public class RoomGeneratorEditor : Editor

{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RoomGenerator generator = (RoomGenerator)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Room Preview", GUILayout.Height(30)))
        {
            // Optional: Destroy previous Way children to avoid duplicates when pressing button multiple times
            // But we'll leave it simple to match the user's request.
            
            generator.GenerateRoom(generator.customWidth, generator.customLength, generator.customHeight);

            // Mark the scene as dirty so Unity knows to save the changes
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
            }
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(LinearTrack))]
    public class LinearTrackEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LinearTrack myScript = (LinearTrack)target;

            // Create button
            if (GUILayout.Button("Auto populate checkpoints"))
            {
                myScript.PopulateCheckpoints();
            }
        }
    }
}

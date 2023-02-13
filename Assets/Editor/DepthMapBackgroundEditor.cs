using Runtime;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(DepthMapBackground))]
    public class DepthMapBackgroundEditor : UnityEditor.Editor
    {
        private DepthMapBackground DepthMapBackground => (DepthMapBackground)target;
    
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate Mesh"))
            {
                DepthMapBackground.GenerateMesh();
            }
        }
    }
}
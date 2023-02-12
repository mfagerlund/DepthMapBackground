using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[CustomEditor(typeof(DepthMapBackground))]
public class DepthMapBackgroundEditor : Editor
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
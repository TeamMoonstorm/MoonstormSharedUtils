using UnityEditor;

namespace RoR2EditorKit.Core.Inspectors
{
    /// <summary>
    /// Inherit from this class to make your own Component Inspectors.
    /// </summary>
    public abstract class ComponentInspector : ExtendedInspector
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("box");
            InspectorEnabled = CreateEnableInsepctorToggle();
            EditorGUILayout.EndVertical();
            base.OnInspectorGUI();
        }
    }
}
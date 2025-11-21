using UnityEditor;

namespace MSU.Editor.Inspectors
{
    [CustomEditor(typeof(UberSkinDef))]
    public class UberSkinDefInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EnsureBaseSkinsCantLoadFromCatalog();
            base.OnInspectorGUI();
        }

        private void EnsureBaseSkinsCantLoadFromCatalog()
        {
            SerializedProperty baseSkinsProperty = serializedObject.FindProperty(nameof(UberSkinDef.baseSkins));
            for(int i = 0; i < baseSkinsProperty.arraySize; i++)
            {
                SerializedProperty addressReferencedSkinDefProperty = baseSkinsProperty.GetArrayElementAtIndex(i);
                SerializedProperty canLoadFromCatalogProperty = addressReferencedSkinDefProperty.FindPropertyRelative("_canLoadFromCatalog");

                if(canLoadFromCatalogProperty.boolValue == true)
                {
                    canLoadFromCatalogProperty.boolValue = false;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
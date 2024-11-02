using RoR2.Editor;

namespace MSU.Editor.EditorWindows
{
    public abstract class MSObjectEditingEditorWindow<TObject> : VisualElementObjectEditingWindow<TObject> where TObject : UnityEngine.Object
    {
        public void SetSourceObject()
        {
            serializedObject = new UnityEditor.SerializedObject(_sourceSerializedObject);
        }
        protected override bool ValidatePath(string path)
        {
            return path.ValidateUXMLPath();
        }
    }
}

using RoR2.Editor;

namespace MSU.Editor.EditorWindows
{
    public abstract class MSObjectEditingEditorWindow<TObject> : VisualElementObjectEditingWindow<TObject> where TObject : UnityEngine.Object
    {
        protected override bool ValidatePath(string path)
        {
            return path.ValidateUXMLPath();
        }
    }
}

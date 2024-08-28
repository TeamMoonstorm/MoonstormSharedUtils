using RoR2.Editor.EditorWindows;

namespace MSU.Editor.EditorWindows
{
    public abstract class MSObjectEditingEditorWindow<TObject> : ObjectEditingEditorWindow<TObject> where TObject : UnityEngine.Object
    {
        protected sealed override bool ValidateUXMLPath(string path)
        {
            return path.ValidateUXMLPath();
        }
    }
}

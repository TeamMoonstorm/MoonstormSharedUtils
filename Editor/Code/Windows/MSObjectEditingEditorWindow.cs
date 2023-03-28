using RoR2EditorKit.EditorWindows;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public abstract class MSObjectEditingEditorWindow<TObject> : ObjectEditingEditorWindow<TObject> where TObject : UnityEngine.Object
    {
        protected sealed override bool ValidateUXMLPath(string path)
        {
            return true;
        }
    }
}

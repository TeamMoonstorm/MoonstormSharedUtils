using RoR2EditorKit.Core.EditorWindows;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public abstract class MSObjectEditingEditorWindow<TObject> : ObjectEditingEditorWindow<TObject> where TObject : UnityEngine.Object
    {
        protected sealed override bool ValidateUXMLPath(string path)
        {
            return path.StartsWith("Packages/teammoonstorm-moonstormsharedutils/Editor");
        }
    }
}

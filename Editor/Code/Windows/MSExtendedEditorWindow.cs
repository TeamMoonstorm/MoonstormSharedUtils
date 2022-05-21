using RoR2EditorKit.Core.EditorWindows;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public abstract class MSExtendedEditorWindow<TObject> : ExtendedEditorWindow<TObject> where TObject : UnityEngine.Object
    {
        protected sealed override bool ValidateUXMLPath(string path)
        {
            return path.StartsWith("Packages/teammoonstorm-moonstormsharedutils/Editor");
        }
    }
}

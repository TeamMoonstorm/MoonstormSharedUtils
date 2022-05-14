using RoR2EditorKit.Core.EditorWindows;

namespace Moonstorm.EditorUtils.EditorWindows
{
    public abstract class MSExtendedEditorWindow : ExtendedEditorWindow
    {
        protected sealed override bool ValidateUXMLPath(string path)
        {
            return path.StartsWith("Packages/teammoonstorm-moonstormsharedutils/Editor");
        }
    }
}

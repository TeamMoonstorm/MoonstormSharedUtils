using UnityEditor;
using UnityEditor.Compilation;

namespace MSU.Editor
{
    public static class MSUEditorUtil
    {
        [MenuItem(MSUConstants.MSUMenuRoot + "Trigger Domain Reload")]
        public static void TriggerDomainReload()
        {
            EditorUtility.RequestScriptReload();
        }
    }
}
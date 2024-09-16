using UnityEditor;
using UnityEditor.Compilation;

namespace MSU.Editor
{
    public static class MSUEditorUtil
    {
        [MenuItem(MSUConstants.MSU_MENU_ROOT + "Trigger Domain Reload")]
        public static void TriggerDomainReload()
        {
            EditorUtility.RequestScriptReload();
        }
    }
}
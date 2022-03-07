using UnityEditor;
using UnityEngine;

namespace NGToolsEditor.NGMissingScriptRecovery
{
	[CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
	public class OverrideMissingGUIEditor : MissingGUIEditor
	{
	}
}
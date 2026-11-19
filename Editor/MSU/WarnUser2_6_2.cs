using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MSU.Editor
{
    [InitializeOnLoad]
    internal static class WarnUser2_7_0
    {
        static WarnUser2_7_0()
        {
            if(EditorPrefs.GetBool(nameof(WarnUser2_7_0), true))
            {
                if(EditorUtility.DisplayDialog("MSU 2.7.0 Notice", "Youre on MSU 2.7.0, which is a Transitory Release\r\nIf you've used MSU before, now it's the time to ensure you're not using any deprecated solutions from it, as MSU 3.0 will be the next release\r\nMore information can be found on the changelog file", "I understand"))
                {
                    EditorPrefs.SetBool(nameof(WarnUser2_7_0), false);
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using ThunderKit.Common.Configuration;
/*
namespace RoR2EditorKit
{
    internal static class R2APIDetector
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            AssemblyReloadEvents.afterAssemblyReload += LookForR2API;
        }

        private static void LookForR2API()
        {
            var assemblyNames = AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetName()).Select(asmName => asmName.Name);
            if(assemblyNames.Contains("R2API") && !ContainsDefine("ROR2EK_R2API"))
            {
                EditorApplication.update += AddDefine;
            }
            else if (ContainsDefine($"ROR2EK_R2API"))
            {
                EditorApplication.update += RemoveDefine;
            }
        }

        private static void AddDefine()
        {
            if (EditorApplication.isUpdating)
                return;

            EditorApplication.update -= AddDefine;
            Debug.Log($"ROR2EK: Adding R2API scripting define.");
            ScriptingSymbolManager.AddScriptingDefine("ROR2EK_R2API");
        }

        private static void RemoveDefine()
        {
            if (EditorApplication.isUpdating)
                return;

            EditorApplication.update -= RemoveDefine;
            Debug.Log($"ROR2EK: Removing R2API scripting define.");
            ScriptingSymbolManager.RemoveScriptingDefine("ROR2EK_R2API");
        }

        //Dear twiner, why isnt this public?
        internal static bool ContainsDefine(string define)
        {
            foreach (BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

                if (!defineSymbols.Contains(define))
                    return false;
            }

            return true;
        }
        static bool IsObsolete(BuildTargetGroup group)
        {
            var attrs = typeof(BuildTargetGroup).GetField(group.ToString()).GetCustomAttributes(typeof(ObsoleteAttribute), false);
            return attrs.Length > 0;
        }
    }
}
*/
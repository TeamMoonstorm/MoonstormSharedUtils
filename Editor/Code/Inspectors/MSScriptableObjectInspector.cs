using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Moonstorm.EditorUtils.Inspectors
{
    public abstract class MSScriptableObjectInspector<T> : ScriptableObjectInspector<T> where T : ScriptableObject
    {
        protected override bool ValidateUXMLPath(string path)
        {
            return path.StartsWith("Packages/teammoonstorm-moonstormsharedutils/Editor");
        }
    }
}

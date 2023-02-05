using RoR2EditorKit.Core.Inspectors;
using UnityEngine;

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

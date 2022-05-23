using RoR2EditorKit.Core.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Moonstorm.EditorUtils.Inspectors
{
    [CustomEditor(typeof(SerializableEliteTierDef))]
    public class SerializableEliteTierDefInspector : IMGUIToVisualElementInspector<SerializableEliteTierDef>
    {

    }
}

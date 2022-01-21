using RoR2;
using RoR2EditorKit.Core.Inspectors;
using UnityEditor;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(ObjectScaleCurve))]
    public class ObjectScaleCurveCustomEditor : ComponentInspector
    {
        public override void DrawCustomInspector()
        {
            SerializedProperty useOverallCurveProp = serializedObject.FindProperty("useOverallCurveOnly");
            DrawField(useOverallCurveProp);
            if (useOverallCurveProp.boolValue)
            {
                DrawField("overallCurve");
            }
            else
            {
                DrawField("curveX");
                DrawField("curveY");
                DrawField("curveZ");
            }
            DrawField("timeMax");
        }
    }
}

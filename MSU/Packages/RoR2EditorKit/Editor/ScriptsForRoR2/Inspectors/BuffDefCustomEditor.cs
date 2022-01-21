using RoR2EditorKit.Core.Inspectors;
using UnityEditor;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(RoR2.BuffDef))]
    public class BuffDefCustomEditor : ScriptableObjectInspector
    {
        public override void DrawCustomInspector()
        {
            DrawField("iconSprite");
            DrawField("buffColor");
            DrawField("canStack");
            DrawField("eliteDef");
            DrawField("isDebuff");
            DrawField("startSfx");
        }
    }
}

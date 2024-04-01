using R2API;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    [CreateAssetMenu(fileName = "New DotBuffDef", menuName = "MSU/DotBuffDef")]
    public class DotBuffDef : BuffDef
    {
        public float interval;
        public float damageCoefficient;
        public SerializableDamageColor damageColor;
        public BuffDef terminalTimedBuff;
        public float terminalTimedBuffDDuration;
        public bool resetTimerOnAdd;

        public DotController.DotDef DotDef { get; private set; } = null;
        public DotController.DotIndex DotIndex { get; private set; } = DotController.DotIndex.None;
        public DotAPI.CustomDotBehaviour DotBehaviour { get; set; }
        public DotAPI.CustomDotVisual DotVisual { get; set; }

        public void Init()
        {
            if (DotDef != null || DotIndex != DotController.DotIndex.None)
                return;

            DoInit();
        }

        private void OnValidate()
        {
            base.OnValidate();
            isDebuff = false;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void DoInit()
        {
            if(isDebuff)
            {
#if DEBUG
                MSULog.Warning($"DotBuffDef {name} is marked as a Debuff, this will cause this DOT to count as 2 debuffs for DeathMark! setting isDebuff to false.");
#endif
                isDebuff = false;
            }
            DamageColorIndex damageColorIndex = DamageColorIndex.Default;
            if(damageColor)
            {
                if(damageColor.DamageColorIndex != DamageColorIndex.Default)
                {
                    ColorsAPI.AddSerializableDamageColor(damageColor);
                }
                damageColorIndex = damageColor.DamageColorIndex;
            }

            DotDef = new DotController.DotDef
            {
                associatedBuff = this,
                resetTimerOnAdd = resetTimerOnAdd,
                damageCoefficient = damageCoefficient,
                damageColorIndex = damageColorIndex,
                interval = interval,
                terminalTimedBuff = terminalTimedBuff,
                terminalTimedBuffDuration = terminalTimedBuffDDuration,
            };
            DotIndex = DotAPI.RegisterDotDef(DotDef, DotBehaviour, DotVisual);
        }
    }
}
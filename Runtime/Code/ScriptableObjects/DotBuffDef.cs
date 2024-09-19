using R2API;
using R2API.ScriptableObjects;
using RoR2;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// A DotBuffDef is an extension of <see cref="BuffDef"/> which allows for easy creation of Dots tied to BuffDefs. This class as well ensures that <see cref="BuffDef.isDebuff"/> is false for this particular DOT to avoid the DOT itself counting as 2 debuffs for DeathMark.
    /// <br>In case you're using vanilla DamageColors or BuffDefs, you can call <see cref="Init"/> and then manually modify the DotDef stored in <see cref="dotDef"/></br>
    /// </summary>
    [CreateAssetMenu(fileName = "New DotBuffDef", menuName = "MSU/DotBuffDef")]
    public class DotBuffDef : BuffDef
    {
        [Header("DOT Metadata")]
        [Tooltip("The time between damage ticks")]
        public float interval;
        [Tooltip("The amount of damage done each damage tick")]
        public float damageCoefficient;
        [Tooltip("A SerializableDamageColor for this DOT, this will be added to the game using R2API.Colors")]
        public SerializableDamageColor damageColor;
        [Tooltip("A BuffDef to apply once this DOT ends")]
        public BuffDef terminalTimedBuff;
        [Tooltip("The duration of the BuffDef that's applied when this DOT ends")]
        public float terminalTimedBuffDuration;
        [Tooltip("If true, the timer of this DOT will be reset upon a new DOTStack being added.")]
        public bool resetTimerOnAdd;

        /// <summary>
        /// The DotDef stored by this DotBuffDef. Returns null if <see cref="Init"/> has not been called
        /// </summary>
        public DotController.DotDef dotDef { get; private set; } = null;
        /// <summary>
        /// The DotIndex that represents <see cref="dotDef"/>
        /// </summary>
        public DotController.DotIndex dotIndex { get; private set; } = DotController.DotIndex.None;

        /// <summary>
        /// <inheritdoc cref="DotAPI.CustomDotBehaviour"/>
        /// </summary>
        public DotAPI.CustomDotBehaviour dotBehaviour { get; set; }

        /// <summary>
        /// <inheritdoc cref="DotAPI.CustomDotVisual"/>
        /// </summary>
        public DotAPI.CustomDotVisual dotVisual { get; set; }

        /// <summary>
        /// Initializes this DotBuffDef, by doing this, the property <see cref="dotDef"/> is created using the metadata stored in this DotBuffDef and then it's added to the game using <see cref="R2API.DotAPI"/>
        /// </summary>
        public void Init()
        {
            if (dotDef != null || dotIndex != DotController.DotIndex.None)
                return;

            DoInit();
        }

        [System.Obsolete]
        private new void OnValidate()
        {
            isDebuff = false;
            iconPath = string.Empty;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void DoInit()
        {
            if (isDebuff)
            {
#if DEBUG
                MSULog.Warning($"DotBuffDef {name} is marked as a Debuff, this will cause this DOT to count as 2 debuffs for DeathMark! setting isDebuff to false.");
#endif
                isDebuff = false;
            }
            DamageColorIndex damageColorIndex = DamageColorIndex.Default;
            if (damageColor)
            {
                if (damageColor.DamageColorIndex != DamageColorIndex.Default)
                {
                    ColorsAPI.AddSerializableDamageColor(damageColor);
                }
                damageColorIndex = damageColor.DamageColorIndex;
            }

            dotDef = new DotController.DotDef
            {
                associatedBuff = this,
                resetTimerOnAdd = resetTimerOnAdd,
                damageCoefficient = damageCoefficient,
                damageColorIndex = damageColorIndex,
                interval = interval,
                terminalTimedBuff = terminalTimedBuff,
                terminalTimedBuffDuration = terminalTimedBuffDuration,
            };
            dotIndex = DotAPI.RegisterDotDef(dotDef, dotBehaviour, dotVisual);
        }
    }
}
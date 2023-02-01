using Moonstorm.AddressableAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.PropertyDrawers
{
    public abstract class AddressableAssetDrawer<T> : PropertyDrawer where T : AddressableAsset
    {
        protected virtual string AddressTooltip { get; }

        protected bool usingDirectReference;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            usingDirectReference = GetDirectReferenceValue(property);

            EditorGUI.BeginProperty(position, label, property);
            var fieldRect = new Rect(position.x, position.y, position.width - 16, position.height);
            EditorGUI.PropertyField(fieldRect,
                usingDirectReference ? property.FindPropertyRelative("asset") : property.FindPropertyRelative("address"),
                new GUIContent(property.displayName, usingDirectReference ? string.Empty : AddressTooltip));

            var contextRect = new Rect(fieldRect.xMax, position.y, 16, position.height);
            EditorGUI.DrawTextureTransparent(contextRect, Constants.MSUIcon, ScaleMode.ScaleToFit);
            if (Event.current.type == EventType.ContextClick)
            {
                Vector2 mousePos = Event.current.mousePosition;
                if (contextRect.Contains(mousePos))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent($"Use Direct Reference"), GetDirectReferenceValue(property), () =>
                    {
                        SetDirectReferenceValue(property, !GetDirectReferenceValue(property));
                    });
                    ModifyContextMenu(menu);
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }
            EditorGUI.EndProperty();
        }

        protected virtual void ModifyContextMenu(GenericMenu menu) { }

        bool GetDirectReferenceValue(SerializedProperty prop)
        {
            return prop.FindPropertyRelative("useDirectReference").boolValue;
        }

        void SetDirectReferenceValue(SerializedProperty prop, bool booleanValue)
        {
            prop.FindPropertyRelative("useDirectReference").boolValue = booleanValue;
            prop.serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(AddressableBuffDef))]
    public sealed class AddressableBuffDefDrawer : AddressableAssetDrawer<AddressableBuffDef>
    { 
        protected override string AddressTooltip => "The Address or Asset Name of the Buff";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
    //----------
    [CustomPropertyDrawer(typeof(AddressableEliteDef))]
    public sealed class AddressableEliteDefDrawer : AddressableAssetDrawer<AddressableEliteDef>
    { 
        protected override string AddressTooltip => "The Address or Asset Name of the Elite";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
    //----------
    [CustomPropertyDrawer(typeof(AddressableEquipmentDef))]
    public sealed class AddressableEquipmentDefDrawer : AddressableAssetDrawer<AddressableEquipmentDef>
    { 
        protected override string AddressTooltip => "The Address or Asset Name of the Equipment";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
    //----------
    [CustomPropertyDrawer(typeof(AddressableExpansionDef))]
    public sealed class AddressableExpansionDefDrawer : AddressableAssetDrawer<AddressableExpansionDef>
    { 
        protected override string AddressTooltip => "The Address or Asset Name of the Expansion";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
    //----------
    [CustomPropertyDrawer(typeof(AddressableGameObject))]
    public sealed class AddressableGameObjectDrawer : AddressableAssetDrawer<AddressableGameObject>
    { 
        protected override string AddressTooltip => "The Address of the GameObject";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
    //----------
    [CustomPropertyDrawer(typeof(AddressableIDRS))]
    public sealed class AddressableIDRSDrawer : AddressableAssetDrawer<AddressableIDRS>
    { 
        protected override string AddressTooltip => "The Address of the IDRS";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
    //----------
    [CustomPropertyDrawer(typeof(AddressableItemDef))]
    public sealed class AddressableItemDefDrawer : AddressableAssetDrawer<AddressableItemDef>
    { 
        protected override string AddressTooltip => "The Address or Asset Name of the Item";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
    //----------
    [CustomPropertyDrawer(typeof(AddressableUnlockableDef))]
    public sealed class AddressableUnlockableDefDrawer : AddressableAssetDrawer<AddressableUnlockableDef>
    { 
        protected override string AddressTooltip => "The Address or Asset Name of the Unlockable";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
    //----------
    [CustomPropertyDrawer(typeof(AddressableSpawnCard))]
    public sealed class AddressableSpawnCardDrawer : AddressableAssetDrawer<AddressableSpawnCard>
    {
        protected override string AddressTooltip => "The Address of the Spawn Card";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
}
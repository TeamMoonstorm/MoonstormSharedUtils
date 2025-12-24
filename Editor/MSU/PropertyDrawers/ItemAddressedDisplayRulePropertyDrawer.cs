using RoR2;
using RoR2.Editor;
using System;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static MSU.ItemDisplayAddressedDictionary;

namespace MSU.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ItemAddressedDisplayRule))]
    public class ItemAddressedDisplayRulePropertyDrawer : IMGUIPropertyDrawer<ItemAddressedDisplayRule>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rectForProperty = new Rect(position.x, position.y, position.width, standardPropertyHeight);
            if (!EditorGUI.PropertyField(rectForProperty, property, label, false))
                return;

            using (new EditorGUI.IndentLevelScope(1))
            {
                var ruleType = property.FindPropertyRelative(nameof(ItemAddressedDisplayRule.ruleType));
                rectForProperty.y += standardPropertyHeight;
                EditorGUI.PropertyField(rectForProperty, ruleType);
                if (ruleType.enumValueIndex == (int)ItemDisplayRuleType.LimbMask)
                {
                    var limbMask = property.FindPropertyRelative(nameof(ItemAddressedDisplayRule.limbMask));
                    rectForProperty.y += standardPropertyHeight;
                    EditorGUI.PropertyField(rectForProperty, limbMask);
                    return;
                }

                var followerPrefab = property.FindPropertyRelative(nameof(ItemAddressedDisplayRule.displayPrefab));
                rectForProperty.y += standardPropertyHeight;
                EditorGUI.PropertyField(rectForProperty, followerPrefab);

                var childName = property.FindPropertyRelative(nameof(ItemAddressedDisplayRule.childName));
                rectForProperty.y += standardPropertyHeight * 2;
                EditorGUI.PropertyField(rectForProperty, childName);

                var localPos = property.FindPropertyRelative(nameof(ItemAddressedDisplayRule.localPos));
                rectForProperty.y += standardPropertyHeight;
                EditorGUI.PropertyField(rectForProperty, localPos);

                var localAngles = property.FindPropertyRelative(nameof(ItemAddressedDisplayRule.localAngles));
                rectForProperty.y += standardPropertyHeight;
                EditorGUI.PropertyField(rectForProperty, localAngles);

                var localScale = property.FindPropertyRelative(nameof(ItemAddressedDisplayRule.localScale));
                rectForProperty.y += standardPropertyHeight;
                EditorGUI.PropertyField(rectForProperty, localScale);

                rectForProperty.y += standardPropertyHeight;
                if (GUI.Button(rectForProperty, "Paste from IDPH"))
                {
                    PasteFromIDPH(childName, localPos, localAngles, localScale);
                }
            }
        }

        private void PasteFromIDPH(SerializedProperty childName, SerializedProperty localPos, SerializedProperty localAngles, SerializedProperty localScale)
        {
            var clipboardContent = EditorGUIUtility.systemCopyBuffer;
            string childNameValue = childName.stringValue;
            Vector3 localPosValue = localPos.vector3Value;
            Vector3 localAnglesValue = localAngles.vector3Value;
            Vector3 localScaleValue = localScale.vector3Value;

            try
            {
                var split = clipboardContent.Split(',').ToArray();
                childNameValue = split[0];
                localPosValue = ParseVector3(split[1], split[2], split[3]);
                localAnglesValue = ParseVector3(split[4], split[5], split[6]);
                localScaleValue = ParseVector3(split[7], split[8], split[9]);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to paste from ItemDisplayPlacementHelper, was the output copied with the format \"forParsing\"?\n{e}");
                return;
            }

            childName.stringValue = childNameValue;
            localPos.vector3Value = localPosValue;
            localAngles.vector3Value = localAnglesValue;
            localScale.vector3Value = localScaleValue;

            Vector3 ParseVector3(string x, string y, string z)
            {
                var invariant = CultureInfo.InvariantCulture;
                return new Vector3(float.Parse(x, invariant), float.Parse(y, invariant), float.Parse(z, invariant));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            base.GetPropertyHeight(property, label);
            var total = standardPropertyHeight; //base foldout property itself.
            if (!property.isExpanded)
                return total;

            total += standardPropertyHeight; //ruletype
            var ruleType = property.FindPropertyRelative(nameof(ItemDisplayRule.ruleType));
            if (ruleType.enumValueIndex == (int)ItemDisplayRuleType.LimbMask)
            {
                total += standardPropertyHeight; //LimbFlags themselves.
                return total;
            }

            total += standardPropertyHeight * 6; //displayPrefab, childName, localPos, localAngles, localScale.
            total += standardPropertyHeight; //Paste button
            return total;
        }
    }
}
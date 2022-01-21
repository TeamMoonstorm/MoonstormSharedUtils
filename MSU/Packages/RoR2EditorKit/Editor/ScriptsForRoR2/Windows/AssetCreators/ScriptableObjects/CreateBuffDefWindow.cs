using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class CreateBuffDefWindow : CreateRoR2ScriptableObjectWindow<BuffDef>
    {
        public BuffDef buff;

        private bool drawExtraSettings = false;
        private string startEventString;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "ScriptableObjects/BuffDef", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateBuffDefWindow>(null, "Create BuffDef");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            buff = (BuffDef)ScriptableObject;
            startEventString = string.Empty;
            buff.iconSprite = Constants.NullSprite;
            mainSerializedObject = new SerializedObject(buff);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box");

            nameField = EditorGUILayout.TextField("Asset Name", nameField);

            DrawField("buffColor");
            DrawField("canStack");
            DrawField("isDebuff");

            SwitchButton("Extra Settings", ref drawExtraSettings);

            if (drawExtraSettings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box");
                DrawExtraBuffSettings();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (SimpleButton("Create Buff"))
            {
                var result = CreateBuff();
                if (result)
                {
                    Debug.Log($"Succesfully Created Buff {nameField}");
                    TryToClose();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawExtraBuffSettings()
        {
            startEventString = EditorGUILayout.TextField("Wwise StartSFX Event", startEventString);
            DrawField("iconSprite");
            DrawField("eliteDef");
        }

        private bool CreateBuff()
        {
            actualName = GetCorrectAssetName(nameField);
            try
            {
                if (string.IsNullOrEmpty(actualName))
                    throw ErrorShorthands.ThrowNullAssetName(nameof(nameField));

                buff.name = actualName;

                if (!string.IsNullOrEmpty(startEventString))
                {
                    buff.startSfx = CreateStartSFX();
                }

                Util.CreateAssetAtSelectionPath(buff);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while creating buff: {e}");
                return false;
            }
        }

        private NetworkSoundEventDef CreateStartSFX()
        {
            var soundEventDef = CreateInstance<NetworkSoundEventDef>();
            soundEventDef.eventName = startEventString;
            soundEventDef.name = actualName + "StartSFX";
            return (NetworkSoundEventDef)Util.CreateAssetAtSelectionPath(soundEventDef);
        }
    }
}
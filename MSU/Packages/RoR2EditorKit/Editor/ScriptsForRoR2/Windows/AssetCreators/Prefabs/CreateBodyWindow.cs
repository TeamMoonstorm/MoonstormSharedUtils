/*using RoR2EditorKit.Core.Windows;
using RoR2.Projectile;
using RoR2EditorKit.Common;
using UnityEditor;
using UnityEngine;
using System;
using RoR2;
using EntityStates;
using System.Collections.Generic;

namespace RoR2EditorKit.RoR2.EditorWindows
{
    public class CreateBodyWindow : CreateRoR2PrefabWindow<CharacterBody>
    {
        private enum BodyWindowEnum
        {
            None,
            Stats,
            AimingAndCrosshair,
            EntityStateMachine,
            CharBodyExtras,
        }

        public CharacterBody body;

        private bool flying = false;
        private bool addSubtitleToken = false;
        private int amountOfSkills = 1;
        private int amountOfStateMachines = 1;

        private BodyWindowEnum currentEnum = BodyWindowEnum.None;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "Prefabs/Body", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateBodyWindow>(null, "Create Basic Body");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            flying = false;
            addSubtitleToken = false;
            amountOfSkills = 1;

            body = MainComponent;
            mainSerializedObject = new SerializedObject(mainPrefab);
        }

        private void OnGUI()
        {
            if (SimpleButton("Set Defaults To CommandoBody Defaults"))
                SetDefaults();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.MaxWidth(300));

            nameField = EditorGUILayout.TextField("Body Name", nameField);
            flying = EditorGUILayout.Toggle("Flying", flying);
            addSubtitleToken = EditorGUILayout.Toggle($"Subtitle Token", addSubtitleToken);
            amountOfSkills = EditorGUILayout.IntSlider("Amount of Skills", amountOfSkills, 1, 4);
            amountOfStateMachines = EditorGUILayout.IntField("Amount of Entity State Machines", amountOfStateMachines);

            DrawField("bodyFlags", serializedComponent);
            DrawField("hullClassification", serializedComponent);
            DrawField("isChampion", serializedComponent);

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.MaxWidth(300));
            if (SimpleButton("Stats"))
            {
                currentEnum = BodyWindowEnum.Stats;
            }
            if(SimpleButton("Aiming and Crosshair"))
            {
                currentEnum = BodyWindowEnum.AimingAndCrosshair;
            }
            if(amountOfStateMachines > 0 && SimpleButton("Entity State Machines"))
            {
                currentEnum = BodyWindowEnum.EntityStateMachine;
            }
            if(SimpleButton("Extra Settings"))
            {
                currentEnum = BodyWindowEnum.CharBodyExtras;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            
            switch(currentEnum)
            {
                case BodyWindowEnum.None:
                    EditorGUILayout.LabelField($"Please click one of the buttons.");
                    break;
                case BodyWindowEnum.Stats:
                    ShowStats();
                    break;
                case BodyWindowEnum.AimingAndCrosshair:
                    ShowAimingAndCrosshair();
                    break;
                case BodyWindowEnum.CharBodyExtras:
                    ShowExtras();
                    break;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }
        private void ShowStats()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            DrawField("baseMaxHealth", serializedComponent);
            DrawField("baseRegen", serializedComponent);
            DrawField("baseMaxShield", serializedComponent);
            DrawField("baseMoveSpeed", serializedComponent);
            DrawField("baseAcceleration", serializedComponent);
            DrawField("baseJumpPower", serializedComponent);
            DrawField("baseDamage", serializedComponent);
            DrawField("baseAttackSpeed", serializedComponent);
            DrawField("baseCrit", serializedComponent);
            DrawField("baseArmor", serializedComponent);
            DrawField("baseJumpCount", serializedComponent);
            DrawField("sprintingSpeedMultiplier", serializedComponent);

            EditorGUILayout.Space();
            DrawField("autoCalculateLevelStats", serializedComponent);
            EditorGUILayout.Space();
            if(!serializedComponent.FindProperty("autoCalculateLevelStats").boolValue)
            {
                DrawField("levelMaxHealth", serializedComponent);
                DrawField("levelRegen", serializedComponent);
                DrawField("levelMaxShield", serializedComponent);
                DrawField("levelMoveSpeed", serializedComponent);
                DrawField("levelJumpPower", serializedComponent);
                DrawField("levelDamage", serializedComponent);
                DrawField("levelAttackSpeed", serializedComponent);
                DrawField("levelCrit", serializedComponent);
                DrawField("levelArmor", serializedComponent);
            }
            EditorGUILayout.EndVertical();

            serializedComponent.ApplyModifiedProperties();
        }

        private void ShowExtras()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

        }

        private void ShowAimingAndCrosshair()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            DrawField("spreadBloomDecayTime", serializedComponent);
            DrawField("spreadBloomCurve", serializedComponent);
            DrawField("crosshairPrefab", serializedComponent);

            EditorGUILayout.EndVertical();

            serializedComponent.ApplyModifiedProperties();
        }

        private void SetDefaults()
        {
            body.bodyFlags |= CharacterBody.BodyFlags.ImmuneToExecutes;
            body.rootMotionInMainState = false;
            body.mainRootSpeed = 0;
            body.baseMaxHealth = 110;
            body.baseRegen = 1;
            body.baseMaxShield = 0;
            body.baseMoveSpeed = 7;
            body.baseAcceleration = 80;
            body.baseJumpPower = 15;
            body.baseDamage = 12;
            body.baseAttackSpeed = 1;
            body.baseCrit = 1;
            body.baseArmor = 0;
            body.baseJumpCount = 1;
            body.sprintingSpeedMultiplier = 1.45f;
            body.autoCalculateLevelStats = true;
            body.spreadBloomDecayTime = 0.7f;
            body.hullClassification = HullClassification.Human;
            body.bodyColor = Color.white;
            body.isChampion = false;
            body.preferredInitialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Uninitialized));

            flying = false;
            addSubtitleToken = true;
            amountOfSkills = 4;
            amountOfStateMachines = 3;

            serializedComponent.Update();
        }
    }
}
*/
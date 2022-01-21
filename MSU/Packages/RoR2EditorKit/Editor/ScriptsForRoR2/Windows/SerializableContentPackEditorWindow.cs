using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests.Datums;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    class SerializableContentPackEditorWindow : ExtendedEditorWindow
    {
        Vector2 scrollPos = new Vector2();
        SerializableContentPack contentPack;
        SerializedProperty mainSelectedProp;
        SerializedProperty mainCurrentProp;
        string selectedArrayPath;

        private AssetBundleDefinitions[] BundleDefinitions { get => Settings.MainManifest.Data.OfType<AssetBundleDefinitions>().ToArray(); }

        private AssemblyDefinitions[] AssemblyDefinitions { get => Settings.MainManifest.Data.OfType<AssemblyDefinitions>().ToArray(); }

        private void OnGUI()
        {
            contentPack = mainSerializedObject.targetObject as SerializableContentPack;
            string[] fieldNames = contentPack.GetType()
                .GetFields()
                .Select(fieldInfo => fieldInfo.Name)
                .Where(strg => strg != "effectDefs")
                .ToArray();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(300), GUILayout.ExpandHeight(true));

            DrawButtonSidebar(fieldNames);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            if (mainSelectedProp != null)
            {
                DrawSelectedArray();
            }
            else
            {
                EditorGUILayout.LabelField("Select an Content Element from the List.");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ApplyChanges();
        }

        private void DrawButtonSidebar(string[] fieldNames)
        {
            foreach (string field in fieldNames)
            {
                if (SimpleButton(ObjectNames.NicifyVariableName(field)))
                {
                    selectedArrayPath = mainSerializedObject.FindProperty(field).propertyPath;
                }
            }
            if (!string.IsNullOrEmpty(selectedArrayPath))
            {
                mainSelectedProp = mainSerializedObject.FindProperty(selectedArrayPath);
            }
        }

        private void DrawSelectedArray()
        {
            mainCurrentProp = mainSelectedProp;

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(500));

            if (SimpleButton($"Populate with all {mainCurrentProp.displayName}"))
            {
                try
                {
                    if (!Settings.MainManifest)
                        ErrorShorthands.ThrowNullMainManifest();

                    switch (mainCurrentProp.name)
                    {
                        case "bodyPrefabs":
                            AddAllPrefabs(typeof(CharacterBody));
                            break;
                        case "masterPrefabs":
                            AddAllPrefabs(typeof(CharacterMaster));
                            break;
                        case "projectilePrefabs":
                            AddAllPrefabs(typeof(ProjectileController));
                            break;
                        case "gameModePrefabs":
                            AddAllPrefabs(typeof(Run));
                            break;
                        case "networkedObjectPrefabs":
                            AddAllPrefabs(typeof(NetworkIdentity), new Type[] { typeof(CharacterBody), typeof(CharacterMaster), typeof(ProjectileController), typeof(Run) });
                            break;
                        case "skillDefs":
                            AddAllScriptableObjects(typeof(SkillDef));
                            break;
                        case "sceneDefs":
                            AddAllScriptableObjects(typeof(SceneDef));
                            break;
                        case "itemDefs":
                            AddAllScriptableObjects(typeof(ItemDef));
                            break;
                        case "equipmentDefs":
                            AddAllScriptableObjects(typeof(EquipmentDef));
                            break;
                        case "buffDefs":
                            AddAllScriptableObjects(typeof(BuffDef));
                            break;
                        case "eliteDefs":
                            AddAllScriptableObjects(typeof(EliteDef));
                            break;
                        case "unlockableDefs":
                            AddAllScriptableObjects(typeof(UnlockableDef));
                            break;
                        case "survivorDefs":
                            AddAllScriptableObjects(typeof(SurvivorDef));
                            break;
                        case "artifactDefs":
                            AddAllScriptableObjects(typeof(ArtifactDef));
                            break;
                        case "surfaceDefs":
                            AddAllScriptableObjects(typeof(SurfaceDef));
                            break;
                        case "networkSoundEventDefs":
                            AddAllScriptableObjects(typeof(NetworkSoundEventDef));
                            break;
                        case "musicTrackDefs":
                            AddAllScriptableObjects(typeof(MusicTrackDef));
                            break;
                        case "gameEndingDefs":
                            AddAllScriptableObjects(typeof(GameEndingDef));
                            break;
                        case "entityStateConfigurations":
                            AddAllScriptableObjects(typeof(EntityStateConfiguration));
                            break;
                        case "entityStateTypes":
                            AddAllEntityStateTypes();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"An error occured while trying to fill the {mainCurrentProp.displayName} Array: {e}");
                }
            }
            EditorGUILayout.PropertyField(mainCurrentProp, true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void AddAllScriptableObjects(Type type)
        {
            var explicitAssets = BundleDefinitions.SelectMany(abd => abd.assetBundles)
                                                  .SelectMany(ab => ab.assets)
                                                  .ToArray();

            var explicitAssetPaths = new List<string>();

            PopulateWithExplicitAssets(explicitAssets, explicitAssetPaths);

            List<ScriptableObject> scriptablesThatMatchType = new List<ScriptableObject>();

            foreach (string assetPath in explicitAssetPaths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                if (asset && asset.GetType() == type)
                {
                    scriptablesThatMatchType.Add(asset as ScriptableObject);
                }
            }

            mainCurrentProp.ClearArray();
            for (int i = 0; i < scriptablesThatMatchType.Count; i++)
            {
                var index = i + 1;
                mainCurrentProp.arraySize = index;
                var prop = mainCurrentProp.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = scriptablesThatMatchType[i];
            }
        }

        private void AddAllPrefabs(Type mainType)
        {
            var explicitAssets = BundleDefinitions.SelectMany(abd => abd.assetBundles)
                                                  .SelectMany(ab => ab.assets)
                                                  .ToArray();

            var explicitAssetPaths = new List<string>();

            PopulateWithExplicitAssets(explicitAssets, explicitAssetPaths);

            List<GameObject> prefabsThatHaveMainType = new List<GameObject>();

            foreach (string path in explicitAssetPaths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset && asset is GameObject go)
                {
                    var component = go.GetComponent(mainType);
                    if (component)
                    {
                        prefabsThatHaveMainType.Add(go);
                    }
                }
            }

            mainCurrentProp.ClearArray();
            for (int i = 0; i < prefabsThatHaveMainType.Count; i++)
            {
                var index = i + 1;
                mainCurrentProp.arraySize = index;
                var prop = mainCurrentProp.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = prefabsThatHaveMainType[i];
            }
        }

        private void AddAllPrefabs(Type mainType, Type[] exclusions)
        {
            var explicitAssets = BundleDefinitions.SelectMany(abd => abd.assetBundles)
                                      .SelectMany(ab => ab.assets)
                                      .ToArray();

            var explicitAssetPaths = new List<string>();

            PopulateWithExplicitAssets(explicitAssets, explicitAssetPaths);

            List<GameObject> prefabsThatHaveMainType = new List<GameObject>();

            foreach (string path in explicitAssetPaths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset && asset is GameObject go)
                {
                    var component = go.GetComponent(mainType);
                    if (component)
                    {
                        bool shouldAdd = true;
                        exclusions.ToList().ForEach(type =>
                        {
                            if (go.GetComponent(type))
                            {
                                shouldAdd = false;
                            }
                        });
                        if (shouldAdd)
                        {
                            prefabsThatHaveMainType.Add(go);
                        }
                    }
                }
            }

            mainCurrentProp.ClearArray();
            for (int i = 0; i < prefabsThatHaveMainType.Count; i++)
            {
                var index = i + 1;
                mainCurrentProp.arraySize = index;
                var prop = mainCurrentProp.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = prefabsThatHaveMainType[i];
            }
        }

        private void AddAllEntityStateTypes()
        {
            List<Type> entityStates = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (AssemblyDefinitions.Any(x => x.definitions.Any(y => asm.Location.Contains(y.name))))
                {
                    asm.GetTypes()
                       .Where(type => typeof(EntityStates.EntityState).IsAssignableFrom(type))
                       .ToList()
                       .ForEach(entityState =>
                       {
                           ArrayUtility.Add(ref contentPack.entityStateTypes, new EntityStates.SerializableEntityStateType(entityState));
                       });
                    mainSerializedObject.Update();
                }
            }
        }

        private void PopulateWithExplicitAssets(IEnumerable<Object> inputAssets, List<string> outputAssets)
        {
            foreach (var asset in inputAssets)
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    var files = Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories);
                    var assets = files.Select(path => AssetDatabase.LoadAssetAtPath<Object>(path));
                    PopulateWithExplicitAssets(assets, outputAssets);
                }
                else if (asset is UnityPackage up)
                {
                    PopulateWithExplicitAssets(up.AssetFiles, outputAssets);
                }
                else
                {
                    outputAssets.Add(assetPath);
                }
            }
        }
    }
}
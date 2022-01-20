/*using RoR2EditorKit.Core.Windows;
using RoR2.Projectile;
using RoR2EditorKit.Common;
using UnityEditor;
using UnityEngine;
using System;

namespace RoR2EditorKit.RoR2.EditorWindows
{
    public class CreateBasicProjectileWindow : CreateRoR2PrefabWindow<ProjectileController>
    {
        [Flags]
        public enum ProjectileComponents : short
        {
            ProjectileSimple = 1,
            ProjectileNetworkTransform = 2,
            ProjectileDamage = 4,
            ProjectileImpactExplosion = 8
        }
        public ProjectileController projectileController;

        public GameObject ghostPrefab;

        public ProjectileComponents components;

        [MenuItem(Constants.RoR2EditorKitContextRoot + "Prefabs/Projectile", false, Constants.RoR2EditorKitContextPriority)]
        public static void Open()
        {
            OpenEditorWindow<CreateBasicProjectileWindow>(null, "Create Basic Projectile");
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            DestroyImmediate(mainPrefab.GetComponent<MeshFilter>());
            DestroyImmediate(mainPrefab.GetComponent<MeshRenderer>());
            ghostPrefab = Constants.NullPrefab;

            projectileController = MainComponent;
            mainSerializedObject = new SerializedObject(mainPrefab);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            nameField = EditorGUILayout.TextField("Projectile Name");
            components = (ProjectileComponents)EditorGUILayout.EnumFlagsField("Components To Add", components);
        }
    }
}
*/
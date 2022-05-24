using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    public abstract class ProjectileModuleBase : ContentModule<ProjectileBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<GameObject, ProjectileBase> MoonstormProjectiles { get; private set; }
        internal static Dictionary<GameObject, ProjectileBase> projectiles = new Dictionary<GameObject, ProjectileBase>();

        public static GameObject[] LoadedProjectiles { get => MoonstormProjectiles.Keys.ToArray(); }
        public static Action<ReadOnlyDictionary<GameObject, ProjectileBase>> OnDictionaryCreated;
        #endregion

        [SystemInitializer(typeof(ProjectileCatalog))]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Projectile Module...");

            MoonstormProjectiles = new ReadOnlyDictionary<GameObject, ProjectileBase>(projectiles);
            projectiles = null;

            OnDictionaryCreated?.Invoke(MoonstormProjectiles);
        }


        #region InitProjectiles
        protected virtual IEnumerable<ProjectileBase> GetProjectileBases()
        {
            MSULog.Debug($"Getting the Projectiles found inside {GetType().Assembly}...");
            return GetContentClasses<ProjectileBase>();
        }

        protected void AddProjectile(ProjectileBase projectile, Dictionary<GameObject, ProjectileBase> projectileDictionary = null)
        {
            InitializeContent(projectile);
            projectileDictionary?.Add(projectile.ProjectilePrefab, projectile);
        }

        protected override void InitializeContent(ProjectileBase contentClass)
        {
            AddSafely(ref SerializableContentPack.projectilePrefabs, contentClass.ProjectilePrefab, "ProjectilePrefabs");
            contentClass.Initialize();

            if(contentClass.ProjectilePrefab.GetComponent<CharacterBody>())
                AddSafely(ref SerializableContentPack.bodyPrefabs, contentClass.ProjectilePrefab, "BodyPrefabs");

            projectiles.Add(contentClass.ProjectilePrefab, contentClass);
            MSULog.Debug($"Projectile {contentClass} Initialized and ensured in {SerializableContentPack.name}");
        }
        #endregion
    }
}

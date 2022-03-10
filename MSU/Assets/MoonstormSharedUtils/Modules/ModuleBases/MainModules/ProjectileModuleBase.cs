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
        public static ReadOnlyDictionary<GameObject, ProjectileBase> MoonstormProjectiles
        {
            get
            {
                if(!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {nameof(MoonstormProjectiles)}", typeof(ProjectileModuleBase));
                    return null;
                }
                return MoonstormProjectiles;
            }
            private set
            {
                MoonstormProjectiles = value;
            }
        }
        private static Dictionary<GameObject, ProjectileBase> projectiles = new Dictionary<GameObject, ProjectileBase>();
        public static Action<ReadOnlyDictionary<GameObject, ProjectileBase>> OnDictionaryCreated;

        public static GameObject[] LoadedProjectiles { get => MoonstormProjectiles.Keys.ToArray(); }
        public static bool Initialized { get; private set; }
        #endregion

        [SystemInitializer(typeof(ProjectileCatalog))]
        private static void SystemInit()
        {
            Initialized = true;
            MSULog.Info("Subscribing to delegates related to projectiles.");

            MoonstormProjectiles = new ReadOnlyDictionary<GameObject, ProjectileBase>(projectiles);
            projectiles.Clear();
            projectiles = null;

            OnDictionaryCreated?.Invoke(MoonstormProjectiles);
        }


        #region InitProjectiles
        protected virtual IEnumerable<ProjectileBase> GetProjectileBases()
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Retrieve ProjectileBase List", typeof(ProjectileModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the Projectiles found inside {GetType().Assembly}...");
            return GetContentClasses<ProjectileBase>();
        }

        protected void AddProjectile(ProjectileBase projectile, Dictionary<GameObject, ProjectileBase> projectileDictionary = null)
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Add ProjectileBase to ContentPack", typeof(ProjectileModuleBase));
                return;
            }

            if (InitializeContent(projectile) && projectileDictionary != null)
                AddSafelyToDict(ref projectileDictionary, projectile.ProjectilePrefab, projectile);

            MSULog.Debug($"Projectile {projectile.ProjectilePrefab} added to {SerializableContentPack.name}");
        }

        protected override bool InitializeContent(ProjectileBase contentClass)
        {
            if(AddSafely(ref SerializableContentPack.projectilePrefabs, contentClass.ProjectilePrefab))
            {
                contentClass.Initialize();

                if (contentClass.ProjectilePrefab.GetComponent<CharacterBody>() && AddSafely(ref SerializableContentPack.bodyPrefabs, contentClass.ProjectilePrefab))
                    MSULog.Debug($"Preemptively added {contentClass.ProjectilePrefab} to CharacterBody array");

                AddSafelyToDict(ref projectiles, contentClass.ProjectilePrefab, contentClass);
                return true;
            }
            return false;
        }
        #endregion
    }
}

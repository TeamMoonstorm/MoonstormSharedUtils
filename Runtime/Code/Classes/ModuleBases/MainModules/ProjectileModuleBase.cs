using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="ProjectileModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="ProjectileBase"/> class
    /// <para><see cref="ProjectileModuleBase"/>'s main job is to handle the proper addition of projectiles specified in <see cref="ProjectileBase"/> inheriting classes</para>
    /// <para>Inherit from this module if you want to load Projectiles with <see cref="ProjectileBase"/> systems</para>
    /// </summary>
    public abstract class ProjectileModuleBase : ContentModule<ProjectileBase>
    {
        #region Properties and Fields}
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific Projectile GameObject by giving it's tied <see cref="ProjectileBase"/>
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<GameObject, ProjectileBase> MoonstormProjectiles { get; private set; }
        internal static Dictionary<GameObject, ProjectileBase> projectiles = new Dictionary<GameObject, ProjectileBase>();

        /// <summary>
        /// Loads all the Projectile GameObjects from the <see cref="MoonstormScenes"/> dictionary.
        /// </summary>
        public static GameObject[] LoadedProjectiles { get => MoonstormProjectiles.Keys.ToArray(); }
        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormUnlockables"/> dictionary has been populated
        /// </summary>
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
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="ProjectileBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="ProjectileBase"/></returns>
        protected virtual IEnumerable<ProjectileBase> GetProjectileBases()
        {
            MSULog.Debug($"Getting the Projectiles found inside {GetType().Assembly}...");
            return GetContentClasses<ProjectileBase>();
        }
        /// <summary>
        /// Adds a ProjectielBase's Projectile GameObject to your mod's ContentPack
        /// </summary>
        /// <param name="projectile">The ProjectielBase to add</param>
        /// <param name="projectileDictionary">Optional, a dictionary to add your initialized ProjectielBase and Projectile Objects</param>
        protected void AddProjectile(ProjectileBase projectile, Dictionary<GameObject, ProjectileBase> projectileDictionary = null)
        {
            InitializeContent(projectile);
            projectileDictionary?.Add(projectile.ProjectilePrefab, projectile);
        }

        /// <summary>
        /// Adds the <see cref="ProjectileBase"/> of <paramref name="contentClass"/> to your mod's SerializableContentPack
        /// <para>Once added, it'll call <see cref="ContentBase.Initialize"/></para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
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

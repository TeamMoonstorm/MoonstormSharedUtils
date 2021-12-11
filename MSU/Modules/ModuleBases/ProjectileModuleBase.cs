using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A module base for managing Projectiles.
    /// </summary>
    public abstract class ProjectileModuleBase : ModuleBase
    {
        /// <summary>
        /// Dictionary of all the projectiles loaded by Moonstorm Shared Utils.
        /// </summary>
        public static readonly Dictionary<GameObject, ProjectileBase> MoonstormProjectiles = new Dictionary<GameObject, ProjectileBase>();

        /// <summary>
        /// Returns all the projectile game objects in MoonstormProjectiles.
        /// </summary>
        public static GameObject[] LoadedProjectiles { get => MoonstormProjectiles.Keys.ToArray(); }

        [SystemInitializer(typeof(ProjectileCatalog))]
        private static void HookInit()
        {
            MSULog.LogI("Subscribing to delegates related to projectiles.");
        }


        #region InitProjectiles
        /// <summary>
        /// Finds all the ProjectileBase inherited classes in your assembly and creates an instance for each found.
        /// <para>Ignores classes with the DisabledContent attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's ProjectileBases.</returns>
        public virtual IEnumerable<ProjectileBase> InitializeProjectiles()
        {
            MSULog.LogD($"Getting the Projectiles found inside {GetType().Assembly}...");
            return GetContentClasses<ProjectileBase>();
        }

        /// <summary>
        /// Initializes and Adds a projectile
        /// </summary>
        /// <param name="projectile">The projectile base class</param>
        /// <param name="contentPack">The content pack of your mod</param>
        /// <param name="projectileDictionary">Optional, a dictionary for getting a projectile base by feeding it the corresponding projectile prefab</param>
        public void AddProjectile(ProjectileBase projectile, SerializableContentPack contentPack, Dictionary<GameObject, ProjectileBase> projectileDictionary = null)
        {
            projectile.Initialize();
            HG.ArrayUtils.ArrayAppend(ref contentPack.projectilePrefabs, projectile.ProjectilePrefab);
            MoonstormProjectiles.Add(projectile.ProjectilePrefab, projectile);
            if (projectileDictionary != null)
                projectileDictionary.Add(projectile.ProjectilePrefab, projectile);

            //Projectiles with characterbody components should be added to the bodyprefab array of the content pack.
            if (projectile.ProjectilePrefab.GetComponent<CharacterBody>())
                HG.ArrayUtils.ArrayAppend(ref contentPack.bodyPrefabs, projectile.ProjectilePrefab);

            MSULog.LogD($"Projectile {projectile.ProjectilePrefab} added to {ContentPack.name}");
        }
        #endregion
    }
}

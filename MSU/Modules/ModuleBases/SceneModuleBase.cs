using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for Managing Scenes
    /// </summary>
    public abstract class SceneModuleBase : ModuleBase
    {
        /// <summary>
        /// Dictionary of all the Scenes loaded by Moonstorm Shared Utils
        /// </summary>
        public static Dictionary<SceneDef, SceneBase> MoonstormScenes = new Dictionary<SceneDef, SceneBase>();

        /// <summary>
        /// Returns all the Scenes loaded by Moonstorm Shared Utils
        /// </summary>
        public SceneDef[] LoadedSceneDefs { get => MoonstormScenes.Keys.ToArray(); }

        [SystemInitializer(typeof(SceneCatalog))]
        private static void HookInit()
        {
            MSULog.LogI("Subscribing to delegates related to stages.");
        }

        #region Scenes

        /// <summary>
        /// Finds all the SceneBase inherited classes in your assembly and creates instances for each found
        /// <para>Ignores classes with the "DisabledContent" attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's SceneBases</returns>
        public virtual IEnumerable<SceneBase> InitializeScenes()
        {
            MSULog.LogD($"Getting the Scenes found inside {GetType().Assembly.GetName().Name}");
            return GetContentClasses<SceneBase>();
        }

        /// <summary>
        /// Initializes and Adds a Scene
        /// </summary>
        /// <param name="scene">The SceneBase class</param>
        /// <param name="contentPack">The content pack of your mod</param>
        /// <param name="sceneDictionary">Optional, a Dictionary for getting a SceneBase by feeding it the corresponding SceneDef</param>
        public void AddScene(SceneBase scene, SerializableContentPack contentPack, Dictionary<SceneDef, SceneBase> sceneDictionary = null)
        {
            scene.Initialize();
            HG.ArrayUtils.ArrayAppend(ref contentPack.sceneDefs, scene.SceneDef);
            MoonstormScenes.Add(scene.SceneDef, scene);

            if (sceneDictionary != null)
                sceneDictionary.Add(scene.SceneDef, scene);

            MSULog.LogD($"Scene {scene.SceneDef} added to {contentPack.name}");
        }
        #endregion
    }
}
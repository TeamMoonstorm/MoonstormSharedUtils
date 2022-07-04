using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="SceneModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="SceneBase"/> class
    /// <para><see cref="SceneModuleBase"/>'s main job is to add new Scenes to the ContentPack. Works with ROS's SceneDefinitions</para>
    /// <para>Inherit from this module if you want to load SceneDefs with SceneBase systems</para>
    /// </summary>
    public abstract class SceneModuleBase : ContentModule<SceneBase>
    {
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific <see cref="SceneBase"/> by giving it's tied <see cref="SceneDef"/>
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<SceneDef, SceneBase> MoonstormScenes { get; private set; }
        internal static Dictionary<SceneDef, SceneBase> scenes = new Dictionary<SceneDef, SceneBase>();

        /// <summary>
        /// Loads all the <see cref="SceneDef"/>s from the <see cref="MoonstormScenes"/> dictionary.
        /// </summary>
        public SceneDef[] LoadedSceneDefs { get => MoonstormScenes.Keys.ToArray(); }
        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormUnlockables"/> dictionary has been populated
        /// </summary>
        public static Action<ReadOnlyDictionary<SceneDef, SceneBase>> OnDictionaryCreated;
        #endregion

        [SystemInitializer(typeof(SceneCatalog))]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Scene Module...");

            MoonstormScenes = new ReadOnlyDictionary<SceneDef, SceneBase>(scenes);
            scenes = null;

            OnDictionaryCreated?.Invoke(MoonstormScenes);
        }

        #region Scenes
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="SceneBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="SceneBase"/></returns>
        public virtual IEnumerable<SceneBase> GetSceneBases()
        {
            MSULog.Debug($"Getting the Scenes found inside {GetType().Assembly.GetName().Name}");
            return GetContentClasses<SceneBase>();
        }

        /// <summary>
        /// Adds a SceneBase's SceneDef to your mod's ContentPack
        /// </summary>
        /// <param name="scene">The Scenebase to add</param>
        /// <param name="sceneDictionary">Opptional, a dictionary to add your initialized SceneBase and SceneDef</param>
        public void AddScene(SceneBase scene, Dictionary<SceneDef, SceneBase> sceneDictionary = null)
        {
            InitializeContent(scene);
            sceneDictionary?.Add(scene.SceneDef, scene);
            MSULog.Debug($"Scene {scene.SceneDef} added to {SerializableContentPack.name}");
        }

        /// <summary>
        /// Adds the <see cref="SceneDef"/> of <paramref name="contentClass"/> to your mod's SerializableContentPack
        /// <para>Once added, it'll call <see cref="ContentBase.Initialize"/></para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
        protected override void InitializeContent(SceneBase contentClass)
        {
            AddSafely(ref SerializableContentPack.sceneDefs, contentClass.SceneDef);
            contentClass.Initialize();
            scenes.Add(contentClass.SceneDef, contentClass);
        }
        #endregion
    }
}
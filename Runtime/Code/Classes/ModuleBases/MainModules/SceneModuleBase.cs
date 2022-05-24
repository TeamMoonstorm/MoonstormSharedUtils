using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Moonstorm
{
    public abstract class SceneModuleBase : ContentModule<SceneBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<SceneDef, SceneBase> MoonstormScenes { get; private set; }
        internal static Dictionary<SceneDef, SceneBase> scenes = new Dictionary<SceneDef, SceneBase>();

        public SceneDef[] LoadedSceneDefs { get => MoonstormScenes.Keys.ToArray(); }
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
        public virtual IEnumerable<SceneBase> GetSceneBases()
        {
            MSULog.Debug($"Getting the Scenes found inside {GetType().Assembly.GetName().Name}");
            return GetContentClasses<SceneBase>();
        }

        public void AddScene(SceneBase scene, Dictionary<SceneDef, SceneBase> sceneDictionary = null)
        {
            InitializeContent(scene);
            sceneDictionary?.Add(scene.SceneDef, scene);
            MSULog.Debug($"Scene {scene.SceneDef} added to {SerializableContentPack.name}");
        }

        protected override void InitializeContent(SceneBase contentClass)
        {
            AddSafely(ref SerializableContentPack.sceneDefs, contentClass.SceneDef);
            contentClass.Initialize();
            scenes.Add(contentClass.SceneDef, contentClass);
        }
        #endregion
    }
}
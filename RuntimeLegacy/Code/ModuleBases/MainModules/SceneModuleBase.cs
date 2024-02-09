using RoR2;
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

        public static ResourceAvailability moduleAvailability;
        #endregion

        [SystemInitializer(typeof(SceneCatalog))]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Scene Module...");

            MoonstormScenes = new ReadOnlyDictionary<SceneDef, SceneBase>(scenes);
            scenes = null;

            moduleAvailability.MakeAvailable();
        }

        #region Scenes

        public virtual IEnumerable<SceneBase> GetSceneBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Scenes found inside {GetType().Assembly.GetName().Name}");
#endif
            return GetContentClasses<SceneBase>();
        }

        public void AddScene(SceneBase scene, Dictionary<SceneDef, SceneBase> sceneDictionary = null)
        {
            InitializeContent(scene);
            sceneDictionary?.Add(scene.SceneDef, scene);
#if DEBUG
            MSULog.Debug($"Scene {scene.SceneDef} added to {SerializableContentPack.name}");
#endif
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
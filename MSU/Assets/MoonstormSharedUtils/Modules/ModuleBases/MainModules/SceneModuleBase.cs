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
        public static ReadOnlyDictionary<SceneDef, SceneBase> MoonstormScenes
        {
            get
            {
                if(!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {nameof(MoonstormScenes)}", typeof(SceneModuleBase));
                    return null;
                }
                return MoonstormScenes;
            }
            private set
            {
                MoonstormScenes = value;
            }
        }
        private static Dictionary<SceneDef, SceneBase> scenes = new Dictionary<SceneDef, SceneBase>();
        public static Action<ReadOnlyDictionary<SceneDef, SceneBase>> OnDictionaryCreated;

        public SceneDef[] LoadedSceneDefs { get => MoonstormScenes.Keys.ToArray(); }
        public static bool Initialized { get; private set; } = false;

        [SystemInitializer(typeof(SceneCatalog))]
        private static void SystemInit()
        {
            Initialized = true;
            MSULog.Info("Subscribing to delegates related to stages.");

            MoonstormScenes = new ReadOnlyDictionary<SceneDef, SceneBase>(scenes);
            scenes.Clear();
            scenes = null;

            OnDictionaryCreated(MoonstormScenes);
        }

        #region Scenes
        public virtual IEnumerable<SceneBase> GetSceneBases()
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Retrieve SceneBase list", typeof(SceneModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the Scenes found inside {GetType().Assembly.GetName().Name}");
            return GetContentClasses<SceneBase>();
        }

        public void AddScene(SceneBase scene, SerializableContentPack contentPack, Dictionary<SceneDef, SceneBase> sceneDictionary = null)
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Add SceneBase to ContentPack", typeof(SceneModuleBase));
                return;
            }

            if (InitializeContent(scene) && sceneDictionary != null)
                AddSafelyToDict(ref sceneDictionary, scene.SceneDef, scene);

            MSULog.Debug($"Scene {scene.SceneDef} added to {contentPack.name}");
        }

        protected override bool InitializeContent(SceneBase contentClass)
        {
            if(AddSafely(ref SerializableContentPack.sceneDefs, contentClass.SceneDef))
            {
                contentClass.Initialize();

                AddSafelyToDict(ref scenes, contentClass.SceneDef, contentClass);
                return true;
            }
            return false;
        }
        #endregion
    }
}
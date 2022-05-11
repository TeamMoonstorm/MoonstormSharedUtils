using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Networking;
using static R2API.ArtifactCodeAPI;

namespace Moonstorm
{
    public abstract class ArtifactModuleBase : ContentModule<ArtifactBase>
    {

        #region Properties and Fields
        public static ReadOnlyDictionary<ArtifactDef, ArtifactBase> MoonstormArtifacts
        {
            get
            {
                if (!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {nameof(MoonstormArtifacts)}", typeof(ArtifactModuleBase));
                    return null;
                }
                return moonstormArtifacts;
            }
            private set
            {
                moonstormArtifacts = value;
            }
        }
        private static ReadOnlyDictionary<ArtifactDef, ArtifactBase> moonstormArtifacts;
        internal static Dictionary<ArtifactDef, ArtifactBase> artifacts = new Dictionary<ArtifactDef, ArtifactBase>();
        public static Action<ReadOnlyDictionary<ArtifactDef, ArtifactBase>> OnDictionaryCreated;
        public static ArtifactDef[] LoadedArtifactDefs { get => MoonstormArtifacts.Keys.ToArray(); }
        public static bool Initialized { get; private set; } = false;
        #endregion

        [SystemInitializer(typeof(ArtifactCatalog))]
        private static void SystemInit()
        {
            Initialized = true;
            MSULog.Info("Initializing Artifact Module...");
            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;

            MoonstormArtifacts = new ReadOnlyDictionary<ArtifactDef, ArtifactBase>(artifacts);
            artifacts = null;

            OnDictionaryCreated?.Invoke(MoonstormArtifacts);
        }



        #region Artifacts
        protected virtual IEnumerable<ArtifactBase> GetArtifactBases()
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Retrieve ArtifactBase list", typeof(ArtifactModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the Artifacts found inside {GetType().Assembly}...");
            return GetContentClasses<ArtifactBase>();
        }

        protected void AddArtifact(ArtifactBase artifact, Dictionary<ArtifactDef, ArtifactBase> artifactDictionary = null)
        {
            if (Initialized)
            {
                ThrowModuleInitialized($"Add ArtifactBase To ContentPack", typeof(ArtifactModuleBase));
                return;
            }

            if (InitializeContent(artifact) && artifactDictionary != null)
                AddSafelyToDict(ref artifactDictionary, artifact.ArtifactDef, artifact);
            
            MSULog.Debug($"Artifact {artifact.ArtifactDef} added to {SerializableContentPack.name}");
        }

        protected override bool InitializeContent(ArtifactBase contentClass)
        {
            if(AddSafely(ref SerializableContentPack.artifactDefs, contentClass.ArtifactDef))
            {
                contentClass.Initialize();

                if (contentClass.ArtifactCode)
                    AddCode(contentClass.ArtifactDef, contentClass.ArtifactCode);

                AddSafelyToDict(ref artifacts, contentClass.ArtifactDef, contentClass);
                return true;
            }
            return false;
        }
        #endregion

        #region Hooks
        private static void OnArtifactEnabled([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            foreach (var kvp in MoonstormArtifacts)
            {
                if (!(artifactDef != kvp.Key) && NetworkServer.active)
                {
                    MSULog.Info($"Running OnArtifactEnabled() for artifact {kvp.Key.cachedName}");
                    kvp.Value.OnArtifactEnabled();
                }
            }
        }

        private static void OnArtifactDisabled([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            foreach (var kvp in MoonstormArtifacts)
            {
                if (!(artifactDef != kvp.Key))
                {
                    MSULog.Info($"Running OnArtifactDisabled() for artifact {kvp.Key.cachedName}");
                    kvp.Value.OnArtifactDisabled();
                }
            }
        }
        #endregion
    }
}

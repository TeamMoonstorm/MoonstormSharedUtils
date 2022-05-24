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
        public static ReadOnlyDictionary<ArtifactDef, ArtifactBase> MoonstormArtifacts { get; private set; }
        internal static Dictionary<ArtifactDef, ArtifactBase> artifacts = new Dictionary<ArtifactDef, ArtifactBase>();

        public static Action<ReadOnlyDictionary<ArtifactDef, ArtifactBase>> OnDictionaryCreated;
        public static ArtifactDef[] LoadedArtifactDefs { get => MoonstormArtifacts.Keys.ToArray(); }
        #endregion

        [SystemInitializer(typeof(ArtifactCatalog))]
        private static void SystemInit()
        {
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
            MSULog.Debug($"Getting the Artifacts found inside {GetType().Assembly}...");
            return GetContentClasses<ArtifactBase>();
        }

        protected void AddArtifact(ArtifactBase artifact, Dictionary<ArtifactDef, ArtifactBase> artifactDictionary = null)
        {
            InitializeContent(artifact);
            artifactDictionary?.Add(artifact.ArtifactDef, artifact);
            
            MSULog.Debug($"Artifact {artifact.ArtifactDef} Initialized and ensured in {SerializableContentPack.name}");
        }

        protected override void InitializeContent(ArtifactBase contentClass)
        {
            AddSafely(ref SerializableContentPack.artifactDefs, contentClass.ArtifactDef);

            contentClass.Initialize();

            if (contentClass.ArtifactCode)
                AddCode(contentClass.ArtifactDef, contentClass.ArtifactCode);

            artifacts[contentClass.ArtifactDef] = contentClass;
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

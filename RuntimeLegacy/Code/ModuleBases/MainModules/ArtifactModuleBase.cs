using RoR2;
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

        public static ArtifactDef[] LoadedArtifactDefs { get => MoonstormArtifacts.Keys.ToArray(); }

        public static ResourceAvailability moduleAvailability;
        #endregion

        [SystemInitializer(typeof(ArtifactCatalog))]
        private static void SystemInit()
        {
            MSULog.Info("Initializing Artifact Module...");
            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;

            MoonstormArtifacts = new ReadOnlyDictionary<ArtifactDef, ArtifactBase>(artifacts);
            artifacts = null;

            moduleAvailability.MakeAvailable();
        }



        #region Artifacts
        protected virtual IEnumerable<ArtifactBase> GetArtifactBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Artifacts found inside {GetType().Assembly}...");
#endif
            return GetContentClasses<ArtifactBase>();
        }

        protected void AddArtifact(ArtifactBase artifact, Dictionary<ArtifactDef, ArtifactBase> artifactDictionary = null)
        {
            InitializeContent(artifact);
            artifactDictionary?.Add(artifact.ArtifactDef, artifact);

#if DEBUG
            MSULog.Debug($"Artifact {artifact.ArtifactDef} Initialized and ensured in {SerializableContentPack.name}");
#endif
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
#if DEBUG
                    MSULog.Info($"Running OnArtifactEnabled() for artifact {kvp.Key.cachedName}");
#endif
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
#if DEBUG
                    MSULog.Info($"Running OnArtifactDisabled() for artifact {kvp.Key.cachedName}");
#endif
                    kvp.Value.OnArtifactDisabled();
                }
            }
        }
        #endregion
    }
}

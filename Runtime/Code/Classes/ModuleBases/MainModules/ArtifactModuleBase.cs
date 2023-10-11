using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Networking;
using static R2API.ArtifactCodeAPI;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="ArtifactModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="ArtifactBase"/> class
    /// <para>The ArtifactModule's main job is to handle the proper addition of ArtifactDefs and proper hooking usage using the game's <see cref="RunArtifactManager"/></para>
    /// <para>ArtifactModule also implements ArtifactCodes using <see cref="R2API.ArtifactCodeAPI"/></para>
    /// <para>Inherit from this module if you want to load and manage Artifacts with <see cref="ArtifactBase"/> systems</para>
    /// </summary>
    public abstract class ArtifactModuleBase : ContentModule<ArtifactBase>
    {

        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific BuffBase by giving it's tied ArtifactDef
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<ArtifactDef, ArtifactBase> MoonstormArtifacts { get; private set; }
        internal static Dictionary<ArtifactDef, ArtifactBase> artifacts = new Dictionary<ArtifactDef, ArtifactBase>();

        /// <summary>
        /// Loads all the Artifactdefs from <see cref="MoonstormArtifacts"/>
        /// </summary>
        public static ArtifactDef[] LoadedArtifactDefs { get => MoonstormArtifacts.Keys.ToArray(); }

        /// <summary>
        /// Call moduleAvailability.CallWhenAvailable() to run a method after the Module is initialized.
        /// </summary>
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
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="ArtifactBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="ArtifactBase"/></returns>
        protected virtual IEnumerable<ArtifactBase> GetArtifactBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the Artifacts found inside {GetType().Assembly}...");
#endif
            return GetContentClasses<ArtifactBase>();
        }

        /// <summary>
        /// Adds an ArtifactBase's ArtifactDef to your mod's content pack
        /// see <see cref="InitializeContent(ArtifactBase)"/>
        /// </summary>
        /// <param name="artifact">The ArtifactBase to add</param>
        /// <param name="artifactDictionary">Optional, a dictionary to add your initialized ArtifactBases and ArtifactDefs</param>
        protected void AddArtifact(ArtifactBase artifact, Dictionary<ArtifactDef, ArtifactBase> artifactDictionary = null)
        {
            InitializeContent(artifact);
            artifactDictionary?.Add(artifact.ArtifactDef, artifact);

#if DEBUG
            MSULog.Debug($"Artifact {artifact.ArtifactDef} Initialized and ensured in {SerializableContentPack.name}");
#endif
        }

        /// <summary>
        /// Initializes an ArtifactBase and adds it to the ContentPack
        /// <para>If the ArtifactBase implements <see cref="ArtifactBase.ArtifactCode"/>, then the code will be added to the game using <see cref="R2API.ArtifactCodeAPI"></para>
        /// <para>Once added, it'll call <see cref="ContentBase.Initialize"/></para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
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

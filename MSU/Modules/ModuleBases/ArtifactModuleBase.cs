using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using R2API;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for Managing Artifacts
    /// </summary>
    public abstract class ArtifactModuleBase : ModuleBase
    {
        /// <summary>
        /// Dictionary of all the Artifacts loaded by Moonstorm Shared Utils
        /// </summary>
        public static Dictionary<ArtifactDef, ArtifactBase> MoonstormArtifacts = new Dictionary<ArtifactDef, ArtifactBase>();

        /// <summary>
        /// Returns all the Artifacts loaded by Moonstorm Shared Utils
        /// </summary>
        public ArtifactDef[] LoadedArtifactDefs { get => MoonstormArtifacts.Keys.ToArray(); }

        [SystemInitializer(typeof(ArtifactCatalog))]
        private static void HookInit()
        {
            MSULog.LogI("Subscribing to delegates related to artifacts.");
            RunArtifactManager.onArtifactEnabledGlobal += OnArtifactEnabled;
            RunArtifactManager.onArtifactDisabledGlobal += OnArtifactDisabled;
        }



        #region Artifacts
        /// <summary>
        /// Finds all the ArtifactBase inherited classes in your assembly and creates instances for each found
        /// <para>Ignores classes with the "DisabledContent" attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your Assembly's ArtifactBases</returns>
        public virtual IEnumerable<ArtifactBase> InitializeArtifacts()
        {
            MSULog.LogD($"Getting the Artifacts found inside {GetType().Assembly}...");
            return GetContentClasses<ArtifactBase>();
        }

        /// <summary>
        /// Initializes and Adds an Artifact
        /// </summary>
        /// <param name="artifact">The ArtifactBase class</param>
        /// <param name="contentPack">The content pack of your mod</param>
        /// <param name="artifactDictionary">Optional, a Dictionary for getting an ArtifactBase by feeding it the corresponding ArtifactDef</param>
        public void AddArtifact(ArtifactBase artifact, SerializableContentPack contentPack, Dictionary<ArtifactDef, ArtifactBase> artifactDictionary = null)
        {
            artifact.Initialize();
            
            HG.ArrayUtils.ArrayAppend(ref contentPack.artifactDefs, artifact.ArtifactDef);
            if (artifact.ArtifactCode != null)
                ArtifactCodeAPI.AddCode(artifact.ArtifactDef, artifact.ArtifactCode);

            MoonstormArtifacts.Add(artifact.ArtifactDef, artifact);
            if (artifactDictionary != null)
                artifactDictionary.Add(artifact.ArtifactDef, artifact);

            MSULog.LogD($"Artifact {artifact.ArtifactDef} added to {contentPack.name}");
        }
        #endregion

        #region Hooks
        private static void OnArtifactEnabled([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            foreach(var kvp in MoonstormArtifacts)
            {
                if(!(artifactDef != kvp.Key) && NetworkServer.active)
                {
                    MSULog.LogI($"Running OnArtifactEnabled() for artifact {kvp.Key.cachedName}");
                    kvp.Value.OnArtifactEnabled();
                }
            }
        }
        private static void OnArtifactDisabled([JetBrains.Annotations.NotNull] RunArtifactManager runArtifactManager, [JetBrains.Annotations.NotNull] ArtifactDef artifactDef)
        {
            foreach(var kvp in MoonstormArtifacts)
            {
                if(!(artifactDef != kvp.Key))
                {
                    MSULog.LogI($"Running OnArtifactDisabled() for artifact {kvp.Key.cachedName}");
                    kvp.Value.OnArtifactDisabled();
                }
            }
        }
        #endregion
    }
}

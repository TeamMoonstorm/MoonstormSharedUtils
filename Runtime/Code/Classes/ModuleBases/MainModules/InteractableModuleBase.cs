using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm
{
    /// <summary>
    /// The <see cref="InteractableModuleBase"/> is a <see cref="ContentModule{T}"/> that handles the <see cref="InteractableBase"/> class
    /// <para><see cref="InteractableModuleBase"/>'s main job is to handle the proper addition of Interactable prefabs from <see cref="InteractableBase"/> inheriting classes</para>
    /// <para><see cref="InteractableBase"/>s that implement <see cref="InteractableBase.InteractableDirectorCard"/> will spawn in runs</para>
    /// <para>Inherit from this module if you want to load and manage Interactables with <see cref="InteractableBase"/> systems</para>
    /// </summary>
    public abstract class InteractableModuleBase : ContentModule<InteractableBase>
    {
        #region Properties and Fields
        /// <summary>
        /// A ReadOnlyDictionary that can be used for loading a specific <see cref="InteractableBase"/> by giving it's tied <see cref="GameObject"/>
        /// <para>If you want to modify classes inside this, subscribe to <see cref="OnDictionaryCreated"/> to ensure the dictionary is not empty</para>
        /// </summary>
        public static ReadOnlyDictionary<GameObject, InteractableBase> MoonstormInteractables { get; private set; }
        internal static Dictionary<GameObject, InteractableBase> interactables = new Dictionary<GameObject, InteractableBase>();

        /// <summary>
        /// Loads all the <see cref="InteractableBase"/> from the <see cref="MoonstormInteractables"/> dictionary that have a <see cref="MSInteractableDirectorCard"/>
        /// </summary>
        public static InteractableBase[] InteractablesWithCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCard != null).ToArray(); }
        /// <summary>
        /// Loads all the <see cref="InteractableBase"/> from the <see cref="MoonstormInteractables"/> dictionary that do not have a <see cref="MSInteractableDirectorCard"/>
        /// </summary>
        public static InteractableBase[] InteractablesWithoutCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCard == null).ToArray(); }
        /// <summary>
        /// Loads all the interactable game objects
        /// </summary>
        public static GameObject[] LoadedInteractables { get => MoonstormInteractables.Keys.ToArray(); }
        /// <summary>
        /// An action that gets invoked when the <see cref="MoonstormInteractables"/> dictionary has been populated
        /// </summary>
        public static Action<ReadOnlyDictionary<GameObject, InteractableBase>> OnDictionaryCreated;
        #endregion

        [SystemInitializer]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Interactable Module...");
            DirectorAPI.InteractableActions += AddCustomInteractables;

            MoonstormInteractables = new ReadOnlyDictionary<GameObject, InteractableBase>(interactables);
            interactables = null;

            OnDictionaryCreated?.Invoke(MoonstormInteractables);
        }


        #region Interactables
        /// <summary>
        /// <inheritdoc cref="ModuleBase{T}.GetContentClasses{T}(Type)"/>
        /// <para>T in this case is <see cref="InteractableBase"/></para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's <see cref="InteractableBase"/></returns>
        protected virtual IEnumerable<InteractableBase> GetInteractableBases()
        {
            MSULog.Debug($"Getting the Interactables found inside {GetType().Assembly}...");
            return GetContentClasses<InteractableBase>();
        }
        /// <summary>
        /// Adds an InteractableBase's InteractablePrefab to the game and to the ContentPack's NetworkedObjects array
        /// </summary>
        /// <param name="interactableBase">The InteractableBase to add</param>
        /// <param name="interactableDictionary">Optional, a dictionary to add your initialized InteractableBase and InteractablePrefab</param>
        protected void AddInteractable(InteractableBase interactableBase, Dictionary<GameObject, InteractableBase> interactableDictionary = null)
        {
            InitializeContent(interactableBase);
            interactableDictionary?.Add(interactableBase.Interactable, interactableBase);
            MSULog.Debug($"Interactable {interactableBase} Initialized and Ensured in {SerializableContentPack.name}");
        }

        /// <summary>
        /// Adds the InteractablePrefab of <paramref name="contentClass"/> to your mod's SerializableContentPack's NetworkedObjects array
        /// <para>Once added, it'll call <see cref="ContentBase.Initialize"/></para>
        /// </summary>
        /// <param name="contentClass">The content class being initialized</param>
        protected override void InitializeContent(InteractableBase contentClass)
        {
            AddSafely(ref SerializableContentPack.networkedObjectPrefabs, contentClass.Interactable, "NetworkedObjectPrefabs");
            contentClass.Initialize();
            interactables.Add(contentClass.Interactable, contentClass);
        }
        #endregion

        #region Hooks
        private static void AddCustomInteractables(DccsPool pool, DirectorAPI.StageInfo stageInfo)
        {
            /*int num = 0;
            foreach (var interactable in InteractablesWithCards)
            {
                var card = interactable.InteractableDirectorCard;
                //Add stage only if the card's stage flags has its corresponding flag.
                if (card.stages.HasFlag(stageInfo.stage))
                {
                    //Stage is custom? check if the custom stage name is in the card's custom stages list.
                    if (stageInfo.stage == DirectorAPI.Stage.Custom)
                    {
                        //If custom stages list contains the current stage's name, add it to the list.
                        if (card.customStages.Contains(stageInfo.CustomStageName.ToLowerInvariant()))
                        {
                            num++;
                            cardList.Add(card.DirectorCardHolder);
                            MSULog.Debug($"Added {card} Interactable");
                            continue;
                        }
                    }
                    else if (stageInfo.CheckStage(card.stages))
                    {
                        num++;
                        cardList.Add(card.DirectorCardHolder);
                        MSULog.Debug($"Added {card} Interactable");
                    }
                }
            }
            if (num > 0)
            {
                MSULog.Debug($"Added a total of {num} Interactables");
            }*/
        }
        #endregion
    }
}

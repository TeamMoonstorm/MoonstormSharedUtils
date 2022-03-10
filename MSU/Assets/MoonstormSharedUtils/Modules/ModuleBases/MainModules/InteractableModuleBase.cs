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
    public abstract class InteractableModuleBase : ContentModule<InteractableBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<GameObject, InteractableBase> MoonstormInteractables
        {
            get
            {
                if(!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {nameof(MoonstormInteractables)}", typeof(InteractableModuleBase));
                    return null;
                }
                return MoonstormInteractables;
            }
            private set
            {
                MoonstormInteractables = value;
            }
        }
        internal static Dictionary<GameObject, InteractableBase> interactables = new Dictionary<GameObject, InteractableBase>();
        public static Action<ReadOnlyDictionary<GameObject, InteractableBase>> OnDictionaryCreated;

        public static InteractableBase[] InteractablesWithCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCard != null).ToArray(); }
        public static InteractableBase[] InteractablesWithoutCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCard == null).ToArray(); }
        public static GameObject[] LoadedInteractables { get => MoonstormInteractables.Keys.ToArray(); }

        public static bool Initialized { get; private set; }
        #endregion

        [SystemInitializer]
        private static void SystemInit()
        {
            Initialized = true;
            MSULog.Info($"Initializing Interactable Module...");
            DirectorAPI.InteractableActions += AddCustomInteractables;

            MoonstormInteractables = new ReadOnlyDictionary<GameObject, InteractableBase>(interactables);
            interactables.Clear();
            interactables = null;

            OnDictionaryCreated?.Invoke(MoonstormInteractables);
        }


        #region Interactables
        protected virtual IEnumerable<InteractableBase> GetInteractableBases()
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Retrieve InteractableBase list", typeof(InteractableModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the Interactables found inside {GetType().Assembly}...");
            return GetContentClasses<InteractableBase>();
        }
        protected void AddInteractable(InteractableBase interactableBase, Dictionary<GameObject, InteractableBase> interactableDictionary = null)
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Add InteractableBase", typeof(InteractableModuleBase));
                return;
            }

            if (InitializeContent(interactableBase) && interactableDictionary != null)
                AddSafelyToDict(ref interactableDictionary, interactableBase.Interactable, interactableBase);

            MSULog.Debug($"Interactable {interactableBase} Added");
        }

        protected override bool InitializeContent(InteractableBase contentClass)
        {
            AddSafely(ref SerializableContentPack.networkedObjectPrefabs, contentClass.Interactable);

            contentClass.Initialize();

            AddSafelyToDict(ref interactables, contentClass.Interactable, contentClass);
            return true;
        }
        #endregion

        #region Hooks
        private static void AddCustomInteractables(List<DirectorAPI.DirectorCardHolder> cardList, DirectorAPI.StageInfo stageInfo)
        {
            int num = 0;
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
            }
        }
        #endregion
    }
}

using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// A Module Base for Managing Interactables
    /// <para>Automatically adds them to the StageDirector according to the settings on the interactable's MSInteractableDirectorCard</para>
    /// </summary>
    public abstract class InteractableModuleBase : ModuleBase
    {
        /// <summary>
        /// Dictionary of all the Interactables loaded by Moonstorm Shared Utils
        /// </summary>
        public static Dictionary<GameObject, InteractableBase> MoonstormInteractables = new Dictionary<GameObject, InteractableBase>();
        
        /// <summary>
        /// Returns all the Interactables loaded by Moonstorm Shared Utils that have an MSInteractableDirectorCard
        /// </summary>
        public static InteractableBase[] InteractablesWithCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCard != null).ToArray(); }

        /// <summary>
        /// Returns all the Interactables loaded by Moonstorm Shared Utils that do not have an MSInteractableDirectorCard
        /// </summary>
        public static InteractableBase[] InteractablesWithoutCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCard == null).ToArray(); }

        /// <summary>
        /// Returns all the Interactables loaded by Moonstorm Shared Utils
        /// </summary>
        public static GameObject[] LoadedInteractables { get => MoonstormInteractables.Keys.ToArray(); }

        [SystemInitializer]
        private static void HookInit()
        {
            MSULog.LogI($"Subscribing to delegates related to Interactables");
            DirectorAPI.InteractableActions += AddCustomInteractables;
        }


        #region Interactables
        /// <summary>
        /// Finds all the InteractableBase inheriting classes in your assembly and creates instances for each found
        /// <para>Ignores classes with the DisabledContent attribute</para>
        /// </summary>
        /// <returns>An IEnumerable of all your assembly's InteractableBases</returns>
        public virtual IEnumerable<InteractableBase> InitializeInteractables()
        {
            MSULog.LogD($"Getting the Interactables found inside {GetType().Assembly}...");
            return GetContentClasses<InteractableBase>();
        }

        /// <summary>
        /// Initializes and Adds an Interactable
        /// </summary>
        /// <param name="interactableBase">The InteractableBase class</param>
        /// <param name="interactableDictionary">Optional, a Dictionary for getting an InteractableBase by feeding it the interactable prefab</param>
        public void AddInteractable(InteractableBase interactableBase, Dictionary<GameObject, InteractableBase> interactableDictionary = null)
        {
            interactableBase.Initialize();

            MoonstormInteractables.Add(interactableBase.Interactable, interactableBase);

            if (interactableDictionary != null)
                interactableDictionary.Add(interactableBase.Interactable, interactableBase);

            MSULog.LogD($"Interactable {interactableBase} Added");
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
                            MSULog.LogD($"Added {card} Interactable");
                            continue;
                        }
                    }
                    else if (stageInfo.CheckStage(card.stages))
                    {
                        num++;
                        cardList.Add(card.DirectorCardHolder);
                        MSULog.LogD($"Added {card} Interactable");
                    }
                }
            }
            if (num > 0)
            {
                MSULog.LogD($"Added a total of {num} Interactables");
            }
        }
        #endregion
    }
}

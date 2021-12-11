using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moonstorm
{
    public abstract class InteractableModuleBase : ModuleBase
    {
        public static Dictionary<GameObject, InteractableBase> MoonstormInteractables = new Dictionary<GameObject, InteractableBase>();

        public static InteractableBase[] InteractablesWithCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCard != null).ToArray(); }

        public static InteractableBase[] InteractablesWithoutCards { get => MoonstormInteractables.Values.Where(ib => ib.InteractableDirectorCard == null).ToArray(); }

        public static GameObject[] LoadedInteractables { get => MoonstormInteractables.Keys.ToArray(); }

        [SystemInitializer]
        private static void HookInit()
        {
            MSULog.LogI($"Subscribing to delegates related to Interactables");
            DirectorAPI.InteractableActions += AddCustomInteractables;
        }


        #region Interactables
        public virtual IEnumerable<InteractableBase> InitializeInteractables()
        {
            MSULog.LogD($"Getting the Interactables found inside {GetType().Assembly}...");
            return GetContentClasses<InteractableBase>();
        }

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

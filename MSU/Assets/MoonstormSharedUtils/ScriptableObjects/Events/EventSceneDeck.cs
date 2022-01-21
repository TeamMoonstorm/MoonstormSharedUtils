using UnityEngine;

namespace Moonstorm.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Moonstorm/Events/Event Scene Deck")]
    public class EventSceneDeck : ScriptableObject
    {
        [Tooltip("The name of the scene that this event deck is made for. Type \"all\" to use it for all stages.")]
        public string sceneName;

        public EventCardDeck sceneDeck;
    }
}

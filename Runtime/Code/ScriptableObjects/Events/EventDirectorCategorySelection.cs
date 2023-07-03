using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Moonstorm
{
    /// <summary>
    /// An <see cref="EventDirectorCategorySelection"/> is the <see cref="Moonstorm.Components.EventDirector"/>'s version of a <see cref="RoR2.DirectorCardCategorySelection"/>
    /// <para>It is used for storing the available events for a stage.</para>
    /// </summary>
    [CreateAssetMenu(menuName = "Moonstorm/Events/EDCS")]
    public class EventDirectorCategorySelection : ScriptableObject
    {
        /// <summary>
        /// Represents an EventCategory
        /// </summary>
        [Serializable]
        public struct EventCategory
        {
            [Tooltip("A name to identify this category.")]
            public string categoryName;

            /// <summary>
            /// The event cards in this category, note that these cannot be filled in the inspector, as they're filled by the <see cref="EventCatalog"/>
            /// </summary>
            [HideInInspector]
            public EventCard[] eventCards;

            [Tooltip("The weight for this category")]
            public float selectionWeight;
        }

        [Tooltip("For what stage this EventDirectorCategorySelection is used in. Set to \"Custom\" for using this in a custom stage")]
        public R2API.DirectorAPI.Stage stage;

        [Tooltip("The name of the custom stage")]
        public string stageName;

        [Tooltip("The categories that this EventDirectorCategorySelection has")]
        [Header("The cards categories cannot be added in the editor\nthey get added at runtime automatically by the EventCatalog")]
        public EventCategory[] categories = Array.Empty<EventCategory>();

        /// <summary>
        /// Sums all the weights in the given category
        /// </summary>
        /// <returns>The sum of all weights</returns>
        public float SumAllWeightsInCategory(EventCategory category)
        {
            float sum = 0f;
            for (int i = 0; i < category.eventCards.Length; i++)
            {
                if (category.eventCards[i].IsAvailable())
                {
                    sum += (float)category.eventCards[i].selectionWeight;
                }
            }
            return sum;
        }

        /// <summary>
        /// Returns a category's index via it's name
        /// </summary>
        /// <param name="categoryName">The name of the category</param>
        /// <returns>The category's index</returns>
        public int FindCategoryIndexByName(string categoryName)
        {
            for (int i = 0; i < categories.Length; i++)
            {
                EventCategory category = categories[i];
                if (string.Compare(category.categoryName, categoryName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Copies all the categories and data from <paramref name="src"/> to this <see cref="EventDirectorCategorySelection"/>
        /// </summary>
        /// <param name="src">The source to copy from</param>
        public void CopyFrom([NotNull] EventDirectorCategorySelection src)
        {
            EventCategory[] array = src.categories;
            Array.Resize(ref categories, src.categories.Length);
            for (int i = 0; i < categories.Length; i++)
            {
                ref EventCategory reference = ref categories[i];
                reference = array[i];
                reference.eventCards = HG.ArrayUtils.Clone(reference.eventCards);
            }
        }

        /// <summary>
        /// Generates the WeightedSelection for this <see cref="EventDirectorCategorySelection"/>
        /// </summary>
        /// <returns>A WeightedSelection of the available EventCards</returns>
        public WeightedSelection<EventCard> GenerateWeightedSelection()
        {
            WeightedSelection<EventCard> weightedSelection = new WeightedSelection<EventCard>();
            for (int i = 0; i < categories.Length; i++)
            {
                ref EventCategory reference = ref categories[i];
                float totalWeight = SumAllWeightsInCategory(reference);
                float actualWeight = reference.selectionWeight / totalWeight;
                if (!(totalWeight > 0f))
                    continue;

                EventCard[] cards = reference.eventCards;
                foreach (EventCard card in cards)
                {
                    if (card.IsAvailable())
                    {
                        weightedSelection.AddChoice(card, card.selectionWeight * actualWeight);
                    }
                }
            }
            return weightedSelection;
        }

        /// <summary>
        /// Removes all categories
        /// </summary>
        public void Clear()
        {
            categories = Array.Empty<EventCategory>();
        }

        /// <summary>
        /// Adds a new EventCategory
        /// </summary>
        /// <param name="name">The name of the category</param>
        /// <param name="selectionWeight">It's selection weight</param>
        /// <returns>The new EventCategory's index</returns>
        public int AddCategory(string name, float selectionWeight)
        {
            EventCategory category = default(EventCategory);
            category.categoryName = name.ToLowerInvariant();
            category.eventCards = Array.Empty<EventCard>();
            category.selectionWeight = selectionWeight;
            EventCategory value = category;
            HG.ArrayUtils.ArrayAppend(ref categories, in value);
            return categories.Length - 1;
        }

        /// <summary>
        /// Adds a new card to a category
        /// </summary>
        /// <param name="categoryIndex">The category index where <paramref name="card"/> will be added</param>
        /// <param name="card">The card to add</param>
        /// <returns>The index of <paramref name="card"/> relative to the Category's <see cref="EventCategory.eventCards"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">When the categoryIndex is out of range</exception>
        public int AddCard(int categoryIndex, EventCard card)
        {
            if (categoryIndex >= categories.Length || categoryIndex < 0)
            {
                throw new ArgumentOutOfRangeException("categoryIndex");
            }
            ref EventCard[] cards = ref categories[categoryIndex].eventCards;
            HG.ArrayUtils.ArrayAppend(ref cards, in card);
            return cards.Length - 1;
        }

        /// <summary>
        /// Removes the cards that fail the predicate specified in <paramref name="predicate"/>
        /// </summary>
        public void RemoveCardsThatFailFilter(Predicate<EventCard> predicate)
        {
            for (int i = 0; i < categories.Length; i++)
            {
                ref EventCategory reference = ref categories[i];
                for (int j = reference.eventCards.Length - 1; j >= 0; j--)
                {
                    EventCard card = reference.eventCards[j];
                    if (!predicate(card))
                    {
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref reference.eventCards, j);
                    }
                }
            }
        }

        /// <summary>
        /// Wether or not this <see cref="EventDirectorCategorySelection"/> is available
        /// </summary>
        /// <returns>True if available, false otherwise</returns>
        public virtual bool IsAvailable()
        {
            return true;
        }

        /// <summary>
        /// This method gets ran when this <see cref="EventDirectorCategorySelection"/> gets chosen by the <see cref="Moonstorm.Components.EventDirector"/>
        /// </summary>
        /// <param name="stageInfo">The current stage</param>
        public virtual void OnSelected(R2API.DirectorAPI.StageInfo stageInfo)
        {
        }

        private void OnValidate()
        {
            for (int i = 0; i < categories.Length; i++)
            {
                EventCategory category = categories[i];
                if (category.selectionWeight <= 0f)
                {
                    MSULog.Error($"{category.categoryName} in {this} has no weight!");
                }
            }
        }
    }
}

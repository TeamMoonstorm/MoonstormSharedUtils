using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Moonstorm
{
    public class EventDirectorCategorySelection : ScriptableObject
    {
        [Serializable]
        public struct EventCategory
        {
            public string categoryName;

            [HideInInspector]
            public EventCard[] eventCards;

            public float selectionWeight;
        }
        public R2API.DirectorAPI.Stage stage;

        public string stageName;

        public EventCategory[] categories = Array.Empty<EventCategory>();

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

        public void Clear()
        {
            categories = Array.Empty<EventCategory>();
        }

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

        public virtual bool IsAvailable()
        {
            return true;
        }

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

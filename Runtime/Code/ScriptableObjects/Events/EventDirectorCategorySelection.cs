using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;

namespace Moonstorm
{
    [CreateAssetMenu(menuName = "Moonstorm/Events/EDCS")]
    public class EventDirectorCategorySelection : ScriptableObject
    {
        [Serializable]
        public struct EventCategory
        {
            [Tooltip("A name to identify this category.")]
            public string categoryName;

            [HideInInspector]
            public EventCard[] eventCards;

            public float selectionWeight;
        }

        public R2API.DirectorAPI.Stage stage;

        public string stageName;

        [Header("The cards categories cannot be added in the editor\nthey get added at runtime automatically by the EventCatalog")]
        public EventCategory[] categories = Array.Empty<EventCategory>();

        public float SumAllWeightsInCategory(EventCategory category)
        {
            float sum = 0f;
            for(int i = 0; i < category.eventCards.Length; i++)
            {
                if(category.eventCards[i].IsAvailable())
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

        public void CopyFrom([NotNull]EventDirectorCategorySelection src)
        {
            EventCategory[] array = src.categories;
            Array.Resize(ref categories, src.categories.Length);
            for(int i = 0; i < categories.Length; i++)
            {
                ref EventCategory reference = ref categories[i];
                reference = array[i];
                reference.eventCards = HG.ArrayUtils.Clone(reference.eventCards);
            }
        }

        public WeightedSelection<EventCard> GenerateWeightedSelection()
        {
            WeightedSelection<EventCard> weightedSelection = new WeightedSelection<EventCard>();
            for(int i = 0; i < categories.Length; i++)
            {
                ref EventCategory reference = ref categories[i];
                float totalWeight = SumAllWeightsInCategory(reference);
                float actualWeight = reference.selectionWeight / totalWeight;
                if (!(totalWeight > 0f))
                    continue;

                EventCard[] cards = reference.eventCards;
                foreach(EventCard card in cards)
                {
                    if(card.IsAvailable())
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
            if((uint)categoryIndex >= categories.Length)
            {
                throw new ArgumentOutOfRangeException("categoryIndex");
            }
            ref EventCard[] cards  = ref categories[categoryIndex].eventCards;
            HG.ArrayUtils.ArrayAppend(ref cards, in card);
            return cards.Length - 1;
        }

        public void RemoveCardsThatFailFilter(Predicate<EventCard> predicate)
        {
            for(int i = 0; i < categories.Length; i++)
            {
                ref EventCategory reference = ref categories[i];
                for(int j = reference.eventCards.Length -1; j >= 0; j--)
                {
                    EventCard card = reference.eventCards[j];
                    if(!predicate(card))
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

        public void OnValidate()
        {
            for(int i = 0; i < categories.Length; i++)
            {
                EventCategory category = categories[i];
                if(category.selectionWeight <= 0f)
                {
                    Debug.LogError($"{category.categoryName} in {this} has no weight!");
                }
            }
        }
    }
}

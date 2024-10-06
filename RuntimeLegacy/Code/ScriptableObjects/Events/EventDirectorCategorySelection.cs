using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Moonstorm
{
    [Obsolete]
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

        public string stageName;

        public EventCategory[] categories = Array.Empty<EventCategory>();

        public float SumAllWeightsInCategory(EventCategory category)
        {
            throw new System.NotImplementedException();
        }

        public int FindCategoryIndexByName(string categoryName)
        {
            throw new System.NotImplementedException();
        }

        public void CopyFrom([NotNull] EventDirectorCategorySelection src)
        {
            throw new System.NotImplementedException();
        }

        public WeightedSelection<EventCard> GenerateWeightedSelection()
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public int AddCategory(string name, float selectionWeight)
        {
            throw new System.NotImplementedException();
        }

        public int AddCard(int categoryIndex, EventCard card)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveCardsThatFailFilter(Predicate<EventCard> predicate)
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsAvailable()
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnSelected(R2API.DirectorAPI.StageInfo stageInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}

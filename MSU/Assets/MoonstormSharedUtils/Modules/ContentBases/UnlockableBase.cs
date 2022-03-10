using RoR2;
using System;
using System.Linq;

namespace Moonstorm
{
    public abstract class UnlockableBase : ContentBase
    {
        public abstract MSUnlockableDef UnlockableDef { get; set; }
        public Type[] RequiredTypes { get; private set; } = Array.Empty<Type>();

        protected void AddRequiredType<T>() where T : ContentBase
        {
            var list = RequiredTypes.ToList();
            list.Add(typeof(T));
            RequiredTypes = list.ToArray();
        }

        public virtual void OnCheckPassed() { }

        public AchievementDef GetAchievementDef { get => UnlockableDef.AchievementDef; }
    }
}

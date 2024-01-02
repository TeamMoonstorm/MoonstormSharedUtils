using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSU
{
    public static class UnlockableManager
    {
        private static List<AchievableUnlockableDef> _achievableUnlockableDefs = new List<AchievableUnlockableDef>();
        
        public static void AddUnlockables(UnlockableDef[] unlockableDefs)
        {
            foreach(UnlockableDef def in unlockableDefs)
            {
                AddUnlockable(def);
            }
        }

        public static void AddUnlockable(UnlockableDef unlockableDef)
        {
            if(unlockableDef is AchievableUnlockableDef aud)
            {
                _achievableUnlockableDefs.Add(aud);
            }
        }

        [SystemInitializer(typeof(UnlockableCatalog))]
        private static void SystemInit()
        {
            MSULog.Info($"Adding AchievableUnlockableDefs to AchievementManager.");
            RoR2BepInExPack.VanillaFixes.SaferAchievementManager.OnCollectAchievementDefs += AddOurs;
        }

        private static void AddOurs(List<string> arg1, Dictionary<string, AchievementDef> arg2, List<AchievementDef> arg3)
        {
            foreach(AchievableUnlockableDef achievableUnlockableDef in _achievableUnlockableDefs)
            {
                var tiedAchievemment = achievableUnlockableDef.TiedAchievementDef;
                achievableUnlockableDef.getHowToUnlockString = () =>
                {
                    return Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", Language.GetString(tiedAchievemment.nameToken), Language.GetString(tiedAchievemment.descriptionToken));
                };
                achievableUnlockableDef.getUnlockedString = () =>
                {
                    return Language.GetStringFormatted("UNLOCKED_FORMAT", Language.GetString(tiedAchievemment.nameToken), Language.GetString(tiedAchievemment.descriptionToken));
                };

                arg1.Add(tiedAchievemment.identifier);
                arg2.Add(tiedAchievemment.identifier, tiedAchievemment);
                arg3.Add(tiedAchievemment);
            }
        }
    }
}
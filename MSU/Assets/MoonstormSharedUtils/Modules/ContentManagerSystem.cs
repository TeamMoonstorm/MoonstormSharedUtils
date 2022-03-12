using Moonstorm.Components;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Moonstorm
{
    public static class ContentManagerSystem
    {
        [SystemInitializer(new Type[] { typeof(BodyCatalog), typeof(ItemCatalog), typeof(EquipmentCatalog), typeof(BuffCatalog) })]
        private static void SystemInit()
        {
            MSULog.Info($"Initializing Content Manager System...");
            foreach(GameObject bodyPrefab in BodyCatalog.allBodyPrefabs)
            {
                var charBody = bodyPrefab.GetComponent<CharacterBody>();
                var manager = bodyPrefab.AddComponent<MoonstormContentManager>();

                manager.Body = charBody;

                if (!charBody.modelLocator)
                    continue;

                if (!charBody.modelLocator.modelTransform)
                    continue;

                if (!charBody.modelLocator.modelTransform.GetComponent<CharacterModel>())
                    continue;

                var eliteBehavior = bodyPrefab.AddComponent<MoonstormEliteBehavior>();
                manager.EliteBehavior = eliteBehavior;

                eliteBehavior.body = charBody;

                eliteBehavior.model = charBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
            }

            CharacterBody.onBodyAwakeGlobal += SetupComponentFields;
            CharacterBody.onBodyStartGlobal += OnBodyStart;
            On.RoR2.CharacterBody.RecalculateStats += OnRecaluclateStats;
            R2API.RecalculateStatsAPI.GetStatCoefficients += OnGetStatCoefficients;
        }


        private static void SetupComponentFields(CharacterBody body)
        {
            var manager = body.GetComponent<MoonstormContentManager>();
            if (!manager)
                return;

            if(body.master)
            {
                manager.HasMaster = true;
                if(body.master.inventory)
                {
                    manager.HasInventory = true;
                }
            }
        }

        private static void OnBodyStart(CharacterBody body)
        {
            var manager = body.GetComponent<MoonstormContentManager>();
            if (!manager)
                return;

            manager.StartGetInterfaces();
        }

        private static void OnRecaluclateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            var manager = self.GetComponent<MoonstormContentManager>();
            manager?.RunStatRecalculationsStart();
            orig(self);
            manager?.RunStatRecalculationsEnd();
        }

        private static void OnGetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var manager = sender.GetComponent<MoonstormContentManager>();
            if (!manager)
                return;

            manager.RunStatHookEventModifiers(args);
        }
    }
}

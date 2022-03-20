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
                try
                {
                    var charBody = bodyPrefab.GetComponent<CharacterBody>();
                    var manager = bodyPrefab.AddComponent<MoonstormContentManager>();

                    var modelLocator = bodyPrefab.GetComponent<ModelLocator>();
                    if (!modelLocator)
                        continue;
                    if (!modelLocator.modelTransform)
                        continue;
                    if (!modelLocator.modelTransform.GetComponent<CharacterModel>())
                        continue;

                    var eliteBehavior = bodyPrefab.AddComponent<MoonstormEliteBehavior>();
                    manager.EliteBehavior = eliteBehavior;

                    eliteBehavior.body = charBody;

                    eliteBehavior.model = modelLocator.modelTransform.GetComponent<CharacterModel>();
                }
                catch(Exception ex)
                {
                    MSULog.Error(ex);
                }
            }
            CharacterBody.onBodyStartGlobal += OnBodyStart;
            On.RoR2.CharacterBody.RecalculateStats += OnRecaluclateStats;
            R2API.RecalculateStatsAPI.GetStatCoefficients += OnGetStatCoefficients;
        }

        private static void OnBodyStart(CharacterBody body)
        {
            var manager = body.GetComponent<MoonstormContentManager>();
            if (!manager)
                return;

            manager.Body = body;
            if (body.master)
            {
                manager.HasMaster = true;
                if (body.master.inventory)
                {
                    manager.HasInventory = true;
                }
            }

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

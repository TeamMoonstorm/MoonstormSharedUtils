﻿using BepInEx;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RoR2.RoR2Content;

namespace MSU
{
    /// <summary>
    /// The EquipmentModule is a Module that handles classes that implement <see cref="IEquipmentContentPiece"/> and <see cref="IEliteContentPiece"/>.
    /// <para>The EquipmentModule's main job is to handle the proper addition of EquipmentDefs and EliteDefs with their respective BuffDefs. The Elites are added using the extra data provided by <see cref="ExtendedEliteDef"/> and <see cref="EliteAPI"/></para>
    /// </summary>
    public static class EquipmentModule
    {
        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding an Equipment's IEquipmentContentPiece.
        /// <br>This Dictionary contains both Elite and NonElite Equipments.</br>
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<EquipmentDef, IEquipmentContentPiece> AllMoonstormEquipments { get; private set; }

        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding an Elite's IEliteContentPiece by using the Elite's EquipmentDef.
        /// <br>This Dictionary contains only Elite Equipments</br>
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<EquipmentDef, IEliteContentPiece> MoonstormEliteEquipments { get; private set; }

        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding a NonElite Equipment's IEquipmentContentPiece.
        /// <br>This Dictionary contains only Non Elite Equipments.</br>
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<EquipmentDef, IEquipmentContentPiece> NonEliteMoonstormEquipments { get; private set; }

        /// <summary>
        /// A ReadOnlyCollection of all the EliteDefs added by MSU.
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</para>
        /// </summary>
        public static ReadOnlyCollection<EliteDef> MoonstormEliteDefs { get; private set; }

        private static Dictionary<EquipmentDef, IEquipmentContentPiece> _moonstormEquipments = new Dictionary<EquipmentDef, IEquipmentContentPiece>();

        /// <summary>
        /// Represents the Availability of this Module.
        /// </summary>
        public static ResourceAvailability moduleAvailability;


        private static Dictionary<BaseUnityPlugin, IEquipmentContentPiece[]> _pluginToEquipments = new Dictionary<BaseUnityPlugin, IEquipmentContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<EquipmentDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<EquipmentDef>>();

        /// <summary>
        /// Adds a new provider to the EquipmentModule.
        /// <br>Fort more info, see <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <param name="plugin">The plugin that's adding the new provider</param>
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateContentPieceProvider{T}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/></param>
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<EquipmentDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        /// <summary>
        /// Obtains all the EquipmentContentPieces that where added by the specified Plugin
        /// </summary>
        /// <param name="plugin">The plugin to obtain it's Equipments</param>
        /// <returns>An array of IEquipmentContentPieces, if the plugin has not added any Equipments, it returns null.</returns>
        public static IEquipmentContentPiece[] GetEquipments(BaseUnityPlugin plugin)
        {
            if (_pluginToEquipments.TryGetValue(plugin, out var items))
            {
                return items;
            }
            return null;
        }

        /// <summary>
        /// A Coroutine used to initialize the Equipments added by <paramref name="plugin"/>
        /// <br>The coroutine yield breaks if the plugin has not added it's specified provider using <see cref="AddProvider(BaseUnityPlugin, IContentPieceProvider{EquipmentDef})"/></br>
        /// </summary>
        /// <param name="plugin">The plugin to initialize it's Equipments</param>
        /// <returns>A Coroutine enumerator that can be Awaited or Yielded</returns>
        public static IEnumerator InitialzeEquipments(BaseUnityPlugin plugin)
        {
            if (_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<EquipmentDef> provider))
            {
                var enumerator = InitializeEquipmentsFromProvider(plugin, provider);

                while (enumerator.MoveNext())
                    yield return null;
            }
            yield break;
        }

        [SystemInitializer(typeof(EquipmentCatalog))]
        private static void SystemInit()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformAction;

            var allEquips = new Dictionary<EquipmentDef, IEquipmentContentPiece>();
            var nonEliteEquips = new Dictionary<EquipmentDef, IEquipmentContentPiece>();
            var eliteEquips = new Dictionary<EquipmentDef, IEliteContentPiece>();
            var eliteDefs = new List<EliteDef>();

            foreach(var(eqpDef, eqp) in _moonstormEquipments)
            {
                allEquips.Add(eqpDef, eqp);
                if(eqp is IEliteContentPiece eliteContent)
                {
                    eliteEquips.Add(eqpDef, eliteContent);
                    eliteDefs.AddRange(eliteContent.EliteDefs);
                }
                else
                {
                    nonEliteEquips.Add(eqpDef, eqp);
                }
            }

            AllMoonstormEquipments = new ReadOnlyDictionary<EquipmentDef, IEquipmentContentPiece>(allEquips);
            MoonstormEliteEquipments = new ReadOnlyDictionary<EquipmentDef, IEliteContentPiece>(eliteEquips);
            NonEliteMoonstormEquipments = new ReadOnlyDictionary<EquipmentDef, IEquipmentContentPiece>(nonEliteEquips);
            MoonstormEliteDefs = new ReadOnlyCollection<EliteDef>(eliteDefs);

            CombatDirector.EliteTierDef[] vanillaTiers = R2API.EliteAPI.VanillaEliteTiers;
            foreach(EliteDef eliteDef in MoonstormEliteDefs)
            {
                if (!(eliteDef is ExtendedEliteDef eed))
                    continue;

                switch(eed.eliteTier)
                {
                    case ExtendedEliteDef.VanillaTier.HonorDisabled:
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[1].eliteTypes, eliteDef);
                        break;
                    case ExtendedEliteDef.VanillaTier.HonorActive:
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[2].eliteTypes, eliteDef);
                        break;
                    case ExtendedEliteDef.VanillaTier.PostLoop:
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[3].eliteTypes, eliteDef);
                        break;
                    case ExtendedEliteDef.VanillaTier.Lunar:
                        HG.ArrayUtils.ArrayAppend(ref vanillaTiers[4].eliteTypes, eliteDef);
                        break;
                }
                R2API.EliteRamp.AddRamp(eed, eed.eliteRamp);
            }

            moduleAvailability.MakeAvailable();
        }

        private static bool PerformAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if(AllMoonstormEquipments.TryGetValue(equipmentDef, out var equipment))
            {
                return equipment.Execute(self);
            }
            return orig(self, equipmentDef);
        }

        private static IEnumerator InitializeEquipmentsFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<EquipmentDef> provider)
        {
            IContentPiece<EquipmentDef>[] content = provider.GetContents();
            List<IContentPiece<EquipmentDef>> equipments = new List<IContentPiece<EquipmentDef>>();

            var helper = new ParallelCoroutineHelper();

            foreach(var equipment in content)
            {
                if (!equipment.IsAvailable(provider.ContentPack))
                    continue;

                equipments.Add(equipment);
                helper.Add(equipment.LoadContentAsync);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            InitializeEquipments(plugin, equipments, provider);
        }

        private static void InitializeEquipments(BaseUnityPlugin plugin, List<IContentPiece<EquipmentDef>> equipments, IContentPieceProvider<EquipmentDef> provider)
        {
            foreach(var equipment in equipments)
            {
                equipment.Initialize();

                var asset = equipment.Asset;
                provider.ContentPack.equipmentDefs.AddSingle(asset);

                if (equipment is IContentPackModifier packModifier)
                {
                    packModifier.ModifyContentPack(provider.ContentPack);
                }

                if (equipment is IEquipmentContentPiece equipmentContentPiece)
                {
                    if (!_pluginToEquipments.ContainsKey(plugin))
                    {
                        _pluginToEquipments.Add(plugin, Array.Empty<IEquipmentContentPiece>());
                    }
                    var array = _pluginToEquipments[plugin];
                    HG.ArrayUtils.ArrayAppend(ref array, equipmentContentPiece);
                    _moonstormEquipments.Add(asset, equipmentContentPiece);
                }

                if (equipment is IEliteContentPiece eliteContentPiece)
                {
                    foreach (var eliteDef in eliteContentPiece.EliteDefs)
                    {
                        provider.ContentPack.eliteDefs.AddSingle(eliteDef);
                    }
                    provider.ContentPack.buffDefs.AddSingle(asset.passiveBuffDef);

                    var eliteDefWithOverlayMaterial = eliteContentPiece.EliteDefs.OfType<ExtendedEliteDef>().FirstOrDefault(eed => eed.overlayMaterial);

                    if(eliteDefWithOverlayMaterial)
                        BuffOverlays.AddBuffOverlay(asset.passiveBuffDef, eliteDefWithOverlayMaterial.overlayMaterial);

                }

                if (equipment is IUnlockableContent unlockableContent)
                {
                    UnlockableDef[] unlockableDefs = unlockableContent.TiedUnlockables;
                    if (unlockableDefs.Length > 0)
                    {
                        UnlockableManager.AddUnlockables(unlockableDefs);
                        provider.ContentPack.unlockableDefs.Add(unlockableDefs);
                    }
                }
            }
        }
    }
}
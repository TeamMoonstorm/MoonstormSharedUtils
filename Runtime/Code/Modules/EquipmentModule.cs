using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

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
        public static ReadOnlyDictionary<EquipmentDef, IEquipmentContentPiece> allMoonstormEquipments { get; private set; }

        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding an Elite's IEliteContentPiece by using the Elite's EquipmentDef.
        /// <br>This Dictionary contains only Elite Equipments</br>
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<EquipmentDef, IEliteContentPiece> moonstormEliteEquipments { get; private set; }

        /// <summary>
        /// A ReadOnlyDictionary that can be used for finding a NonElite Equipment's IEquipmentContentPiece.
        /// <br>This Dictionary contains only Non Elite Equipments.</br>
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<EquipmentDef, IEquipmentContentPiece> nonEliteMoonstormEquipments { get; private set; }

        /// <summary>
        /// A ReadOnlyCollection of all the EliteDefs added by MSU.
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</para>
        /// </summary>
        public static ReadOnlyCollection<EliteDef> moonstormEliteDefs { get; private set; }

        /// <summary>
        /// A ReadOnlyDictionary of the Effect Prefabs associated to Elite Indices.
        /// <para>Subscribe to <see cref="moduleAvailability"/> to ensure the dictionary is not empty.</para>
        /// </summary>
        public static ReadOnlyDictionary<EliteIndex, GameObject> eliteIndexToEffectPrefab { get; private set; }

        private static Dictionary<EquipmentDef, IEquipmentContentPiece> _moonstormEquipments = new Dictionary<EquipmentDef, IEquipmentContentPiece>();

        /// <summary>
        /// Represents the Availability of this Module.
        /// </summary>
        public static ResourceAvailability moduleAvailability;


        private static Dictionary<BaseUnityPlugin, IEquipmentContentPiece[]> _pluginToEquipments = new Dictionary<BaseUnityPlugin, IEquipmentContentPiece[]>();
        private static Dictionary<BaseUnityPlugin, IContentPieceProvider<EquipmentDef>> _pluginToContentProvider = new Dictionary<BaseUnityPlugin, IContentPieceProvider<EquipmentDef>>();

        /// <summary>
        /// Adds a new provider to the EquipmentModule.
        /// <br>For more info, see <see cref="IContentPieceProvider"/></br>
        /// </summary>
        /// <param name="plugin">The plugin that's adding the new provider</param>
        /// <param name="provider">The provider from the plugin, can be one created using <see cref="ContentUtil.CreateGenericContentPieceProvider{T}(BaseUnityPlugin, RoR2.ContentManagement.ContentPack)"/></param>
        public static void AddProvider(BaseUnityPlugin plugin, IContentPieceProvider<EquipmentDef> provider)
        {
            _pluginToContentProvider.Add(plugin, provider);
        }

        /// <summary>
        /// Obtains all the EquipmentContentPieces that where added by the specified Plugin
        /// </summary>
        /// <param name="plugin">The plugin to obtain it's Equipments</param>
        /// <returns>An array of IEquipmentContentPieces, if the plugin has not added any Equipments, it returns an empty Array.</returns>
        public static IEquipmentContentPiece[] GetEquipments(BaseUnityPlugin plugin)
        {
            if (_pluginToEquipments.TryGetValue(plugin, out var items))
            {
                return items;
            }
#if DEBUG
            MSULog.Info($"{plugin} has no registered equipments");
#endif
            return Array.Empty<IEquipmentContentPiece>();
        }

        /// <summary>
        /// A Coroutine used to initialize the Equipments added by <paramref name="plugin"/>
        /// <br>The coroutine yield breaks if the plugin has not added it's specified provider using <see cref="AddProvider(BaseUnityPlugin, IContentPieceProvider{EquipmentDef})"/></br>
        /// </summary>
        /// <param name="plugin">The plugin to initialize it's Equipments</param>
        /// <returns>A Coroutine enumerator that can be Awaited or Yielded</returns>
        public static IEnumerator InitializeEquipments(BaseUnityPlugin plugin)
        {
#if DEBUG
            if (!_pluginToContentProvider.ContainsKey(plugin))
            {
                MSULog.Info($"{plugin} has no IContentPieceProvider registered in the EquipmentModule.");
            }
#endif
            if (_pluginToContentProvider.TryGetValue(plugin, out IContentPieceProvider<EquipmentDef> provider))
            {
                var enumerator = InitializeEquipmentsFromProvider(plugin, provider);

                while (enumerator.MoveNext())
                    yield return new WaitForEndOfFrame();
            }
            yield break;
        }

        [SystemInitializer(typeof(EquipmentCatalog))]
        private static IEnumerator SystemInit()
        {
            MSULog.Info("Initializing the Equipment Module...");

            var allEquips = new Dictionary<EquipmentDef, IEquipmentContentPiece>();
            var nonEliteEquips = new Dictionary<EquipmentDef, IEquipmentContentPiece>();
            var eliteEquips = new Dictionary<EquipmentDef, IEliteContentPiece>();
            var eliteIndexToEffect = new Dictionary<EliteIndex, GameObject>();
            var eliteDefs = new List<EliteDef>();

            foreach (var (eqpDef, eqp) in _moonstormEquipments)
            {
                yield return new WaitForEndOfFrame();
                allEquips.Add(eqpDef, eqp);
                if (eqp is IEliteContentPiece eliteContent)
                {
                    eliteEquips.Add(eqpDef, eliteContent);
                    eliteDefs.AddRange(eliteContent.eliteDefs);
                    foreach(var eliteDef in eliteContent.eliteDefs)
                    {
                        yield return new WaitForEndOfFrame();
                        if(eliteDef is ExtendedEliteDef eed && eed.effect)
                        {
                            eliteIndexToEffect.Add(eed.eliteIndex, eed.effect);
                        }
                    }
                }
                else
                {
                    nonEliteEquips.Add(eqpDef, eqp);
                }
            }

            allMoonstormEquipments = new ReadOnlyDictionary<EquipmentDef, IEquipmentContentPiece>(allEquips);
            moonstormEliteEquipments = new ReadOnlyDictionary<EquipmentDef, IEliteContentPiece>(eliteEquips);
            nonEliteMoonstormEquipments = new ReadOnlyDictionary<EquipmentDef, IEquipmentContentPiece>(nonEliteEquips);
            moonstormEliteDefs = new ReadOnlyCollection<EliteDef>(eliteDefs);
            eliteIndexToEffectPrefab = new ReadOnlyDictionary<EliteIndex, GameObject>(eliteIndexToEffect);

            CombatDirector.EliteTierDef[] vanillaTiers = R2API.EliteAPI.VanillaEliteTiers;
            foreach (EliteDef eliteDef in moonstormEliteDefs)
            {
                yield return new WaitForEndOfFrame();

                if (eliteDef is not ExtendedEliteDef eed)
                    continue;

                switch (eed.eliteTier)
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

            if(allMoonstormEquipments.Count == 0)
            {
#if DEBUG
                MSULog.Info("Not doing EquipmentModule hooks since no equipments are registered.");
#endif
                yield break;
            }
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformAction;
            On.RoR2.CharacterBody.OnEquipmentLost += CallOnEquipmentLost;
            On.RoR2.CharacterBody.OnEquipmentGained += CallOnEquipmentGained;
        }

        private static void HandleEliteOverrides(MonoMod.Cil.ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            bool success = cursor.TryGotoNext(x => x.MatchLdarg(0),
                x => x.MatchLdnull(),
                x => x.MatchStfld<CharacterModel>(nameof(CharacterModel.particleMaterialOverride)));

            if(!success)
            {
                MSULog.Fatal("Failed to match the required instructions for Elite light and particle material replacements!");
                IL.RoR2.CharacterModel.InstanceUpdate -= HandleEliteOverrides;
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<CharacterModel>>(HandleEliteOverridesInternal);

            void HandleEliteOverridesInternal(CharacterModel characterModel)
            {
                var def = EliteCatalog.GetEliteDef(characterModel.myEliteIndex);

                if(def is ExtendedEliteDef ed)
                {
                    characterModel.lightColorOverride = ed.applyLightColorOverrides ? ed.color : null;
                    characterModel.particleMaterialOverride = ed.particleReplacementMaterial;
                }
            }
        }

        private static void CallOnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            if (allMoonstormEquipments.TryGetValue(equipmentDef, out var contentPiece))
            {
                contentPiece.OnEquipmentObtained(self);
            }
        }

        private static void CallOnEquipmentLost(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            orig(self, equipmentDef);
            if (allMoonstormEquipments.TryGetValue(equipmentDef, out var contentPiece))
            {
                contentPiece.OnEquipmentLost(self);
            }
        }

        private static bool PerformAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (allMoonstormEquipments.TryGetValue(equipmentDef, out var equipment))
            {
                return equipment.Execute(self);
            }
            return orig(self, equipmentDef);
        }

        private static IEnumerator InitializeEquipmentsFromProvider(BaseUnityPlugin plugin, IContentPieceProvider<EquipmentDef> provider)
        {
            IContentPiece<EquipmentDef>[] content = provider.GetContents();
            List<IContentPiece<EquipmentDef>> equipments = new List<IContentPiece<EquipmentDef>>();

            var helper = new ParallelMultiStartCoroutine();

            foreach (var equipment in content)
            {
                if (!equipment.IsAvailable(provider.contentPack))
                    continue;

                equipments.Add(equipment);
                helper.Add(equipment.LoadContentAsync);
            }

            helper.Start();
            while (!helper.IsDone())
                yield return new WaitForEndOfFrame();

            InitializeEquipments(plugin, equipments, provider);
        }

        private static void InitializeEquipments(BaseUnityPlugin plugin, List<IContentPiece<EquipmentDef>> equipments, IContentPieceProvider<EquipmentDef> provider)
        {
            foreach (var equipment in equipments)
            {
#if DEBUG
                try
                {
#endif
                    equipment.Initialize();

                    var asset = equipment.asset;
                    provider.contentPack.equipmentDefs.AddSingle(asset);

                    if (equipment is IContentPackModifier packModifier)
                    {
                        packModifier.ModifyContentPack(provider.contentPack);
                    }

                    if (equipment is IEquipmentContentPiece equipmentContentPiece)
                    {
                        if (!_pluginToEquipments.ContainsKey(plugin))
                        {
                            _pluginToEquipments.Add(plugin, Array.Empty<IEquipmentContentPiece>());
                        }
                        var array = _pluginToEquipments[plugin];
                        HG.ArrayUtils.ArrayAppend(ref array, equipmentContentPiece);
                        _pluginToEquipments[plugin] = array;

                        _moonstormEquipments.Add(asset, equipmentContentPiece);
                    }

                    if (equipment is IEliteContentPiece eliteContentPiece)
                    {
                        foreach (var eliteDef in eliteContentPiece.eliteDefs)
                        {
                            provider.contentPack.eliteDefs.AddSingle(eliteDef);
                        }
                        provider.contentPack.buffDefs.AddSingle(asset.passiveBuffDef);

                        var eliteDefWithOverlayMaterial = eliteContentPiece.eliteDefs.OfType<ExtendedEliteDef>().FirstOrDefault(eed => eed.overlayMaterial);

                        if (eliteDefWithOverlayMaterial)
                            BuffOverlays.AddBuffOverlay(asset.passiveBuffDef, eliteDefWithOverlayMaterial.overlayMaterial);

                    }

#if DEBUG
                    MSULog.Info($"Equipment {equipment.GetType().FullName} initialized.");
#endif

#if DEBUG
                }
                catch (Exception ex)
                {
                    MSULog.Error($"Equipment {equipment.GetType().FullName} threw an exception while initializing.\n{ex}");
                }
#endif
            }
        }
    }
}
using R2API.AddressReferencedAssets;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static RoR2.CombatDirector;

namespace Moonstorm
{
    public abstract class EliteTierDefModuleBase : ModuleBase<EliteTierDefBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<SerializableEliteTierDef, EliteTierDefBase> MoonstormEliteTiers { get; private set; }
        internal static Dictionary<SerializableEliteTierDef, EliteTierDefBase> eliteTierDefs = new Dictionary<SerializableEliteTierDef, EliteTierDefBase>();

        public static SerializableEliteTierDef[] LoadedEliteTierDefs { get => MoonstormEliteTiers.Keys.ToArray(); }

        public static ResourceAvailability moduleAvailability;
        #endregion

        [SystemInitializer]
        private static void SystemInit()
        {
            AddressReferencedAsset.OnAddressReferencedAssetsLoaded += () =>
            {
                MSULog.Info($"Initializing EliteTierDef Module...");

                CreateAndAddTiers();

                MoonstormEliteTiers = new ReadOnlyDictionary<SerializableEliteTierDef, EliteTierDefBase>(eliteTierDefs);
                eliteTierDefs = null;

                moduleAvailability.MakeAvailable();
            };
        }

        private static void CreateAndAddTiers()
        {
            foreach (var eliteTierDefBase in eliteTierDefs.Values)
            {
                SerializableEliteTierDef serializableEliteTierDef = eliteTierDefBase.SerializableEliteTierDef;
                serializableEliteTierDef.Create();
                serializableEliteTierDef.EliteTierDef.isAvailable = eliteTierDefBase.IsAvailable;

                R2API.EliteAPI.AddCustomEliteTier(serializableEliteTierDef.EliteTierDef);
            }
        }

        #region EliteTierDefs
        protected virtual IEnumerable<EliteTierDefBase> GetEliteTierDefBases()
        {
#if DEBUG
            MSULog.Debug($"Getting the EliteTierDefs found inside {GetType().Assembly}...");
#endif
            return GetContentClasses<EliteTierDefBase>();
        }

        protected void AddEliteTierDef(EliteTierDefBase eliteTierDef, Dictionary<SerializableEliteTierDef, EliteTierDefBase> dictionary = null)
        {
            InitializeContent(eliteTierDef);
            dictionary?.Add(eliteTierDef.SerializableEliteTierDef, eliteTierDef);
#if DEBUG
            MSULog.Debug($"EliteTierDef {eliteTierDef.SerializableEliteTierDef} added to the game");
#endif
        }

        protected override void InitializeContent(EliteTierDefBase contentClass)
        {
            contentClass.Initialize();
            eliteTierDefs[contentClass.SerializableEliteTierDef] = contentClass;
        }
        #endregion
    }
}

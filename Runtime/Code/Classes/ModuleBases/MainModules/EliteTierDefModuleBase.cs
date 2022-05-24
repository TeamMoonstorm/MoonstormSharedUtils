using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoR2.CombatDirector;

namespace Moonstorm
{
    public abstract class EliteTierDefModuleBase : ModuleBase<EliteTierDefBase>
    {
        #region Properties and Fields
        public static ReadOnlyDictionary<SerializableEliteTierDef, EliteTierDefBase> MoonstormEliteTiers { get; private set; }
        internal static Dictionary<SerializableEliteTierDef, EliteTierDefBase> eliteTierDefs = new Dictionary<SerializableEliteTierDef, EliteTierDefBase>();
        
        public static SerializableEliteTierDef[] LoadedEliteTierDefs { get => MoonstormEliteTiers.Keys.ToArray(); }
        public static Action<ReadOnlyDictionary<SerializableEliteTierDef, EliteTierDefBase>> OnDictionaryCreated;
        #endregion

        [SystemInitializer]
        private static void SystemInit()
        {
            AddressableAssets.AddressableAsset.OnAddressableAssetsLoaded += () =>
            {
                MSULog.Info($"Initializing EliteTierDef Module...");

                CreateAndAddTiers();

                MoonstormEliteTiers = new ReadOnlyDictionary<SerializableEliteTierDef, EliteTierDefBase>(eliteTierDefs);
                eliteTierDefs = null;

                OnDictionaryCreated?.Invoke(MoonstormEliteTiers);
            };
        }

        private static void CreateAndAddTiers()
        {
            foreach(var eliteTierDefBase in eliteTierDefs.Values)
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
            MSULog.Debug($"Getting the EliteTierDefs found inside {GetType().Assembly}...");
            return GetContentClasses<EliteTierDefBase>();
        }

        protected void AddEliteTierDef(EliteTierDefBase eliteTierDef, Dictionary<SerializableEliteTierDef, EliteTierDefBase> dictionary = null)
        {
            InitializeContent(eliteTierDef);
            dictionary?.Add(eliteTierDef.SerializableEliteTierDef, eliteTierDef);
            MSULog.Debug($"EliteTierDef {eliteTierDef.SerializableEliteTierDef} added to the game");
        }

        protected override void InitializeContent(EliteTierDefBase contentClass)
        {
            contentClass.Initialize();
            eliteTierDefs[contentClass.SerializableEliteTierDef] = contentClass;
        }
        #endregion
    }
}

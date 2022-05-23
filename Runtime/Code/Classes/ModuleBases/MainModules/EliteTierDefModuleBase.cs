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
        public static ReadOnlyDictionary<SerializableEliteTierDef, EliteTierDefBase> MoonstormEliteTiers
        {
            get
            {
                if(!Initialized)
                {
                    ThrowModuleNotInitialized($"Retrieve dictionary {nameof(MoonstormEliteTiers)}", typeof(EliteTierDefModuleBase));
                    return null;
                }
                return moonstormEliteTiers;
            }
            private set
            {
                moonstormEliteTiers = value;
            }
        }
        private static ReadOnlyDictionary<SerializableEliteTierDef, EliteTierDefBase> moonstormEliteTiers;
        internal static Dictionary<SerializableEliteTierDef, EliteTierDefBase> eliteTierDefs = new Dictionary<SerializableEliteTierDef, EliteTierDefBase>();
        public static Action<ReadOnlyDictionary<SerializableEliteTierDef, EliteTierDefBase>> OnDictionaryCreated;
        public static SerializableEliteTierDef[] LoadedEliteTierDefs { get => MoonstormEliteTiers.Keys.ToArray(); }
        public static bool Initialized { get; private set; } = false;
        #endregion

        [SystemInitializer]
        private static void SystemInit()
        {
            AddressableAssets.AddressableAsset.OnAddressableAssetsLoaded += () =>
            {
                Initialized = true;
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
            if(Initialized)
            {
                ThrowModuleInitialized($"Retrieve EliteTierDefBase list", typeof(EliteTierDefModuleBase));
                return null;
            }

            MSULog.Debug($"Getting the EliteTierDefs found inside {GetType().Assembly}...");
            return GetContentClasses<EliteTierDefBase>();
        }

        protected void AddEliteTierDef(EliteTierDefBase eliteTierDef, Dictionary<SerializableEliteTierDef, EliteTierDefBase> dictionary = null)
        {
            if(Initialized)
            {
                ThrowModuleInitialized($"Add EliteTierDefBase to Game", typeof(EliteTierDefModuleBase));
                return;
            }

            if (InitializeContent(eliteTierDef) && dictionary != null)
                AddSafelyToDict(ref dictionary, eliteTierDef.SerializableEliteTierDef, eliteTierDef);

            MSULog.Debug($"EliteTierDef {eliteTierDef.SerializableEliteTierDef} added to the game");
        }

        protected override bool InitializeContent(EliteTierDefBase contentClass)
        {
            contentClass.Initialize();
            AddSafelyToDict(ref eliteTierDefs, contentClass.SerializableEliteTierDef, contentClass);
            return true;
        }
        #endregion
    }
}

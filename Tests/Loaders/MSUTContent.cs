using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moonstorm.Loaders;
using R2API.ScriptableObjects;
using RoR2;

namespace Moonstorm
{
    public class MSUTContent : ContentLoader<MSUTContent>
    {
        public static class Items
        {
            public static ItemDef GenericItem;
            public static ItemDef GenericVoidItem;
        }
        public static class Buffs
        {
            public static BuffDef bdGenericBuff;
        }

        public override string identifier => MSUTestsMain.GUID;

        public override R2APISerializableContentPack SerializableContentPack { get; protected set; } = MSUTAssets.LoadAsset<R2APISerializableContentPack>("MSUTestsContent");
        public override Action[] LoadDispatchers { get; protected set; }
        public override Action[] PopulateFieldsDispatchers { get; protected set; }

        public override void Init()
        {
            MSUTLog.Info("Content Loader initialized");
            base.Init();
            LoadDispatchers = new Action[]
            {
                () =>
                {
                    MSUTLog.Info($"Loading routine, initializing items");
                    new Modules.Items().Initialize();
                },
                () =>
                {
                    MSUTLog.Info($"Loading routine, initializing buffs");
                    new Modules.Buffs().Initialize();
                },
                () =>
                {
                    MSUTLog.Info($"Loading routine, swapping shaders");
                    MSUTAssets.Instance.SwapMaterialShaders();
                }
            };

            PopulateFieldsDispatchers = new Action[]
            {
                () =>
                {
                    MSUTLog.Info($"Population routine, populating items");
                    PopulateTypeFields(typeof(Items), ContentPack.itemDefs);
                },
                () =>
                {
                    MSUTLog.Info($"Population routine, Populating buffs");
                    PopulateTypeFields(typeof(Buffs), ContentPack.buffDefs);
                }
            };

            MSUTLog.Info("Dispatchers populated");
        }
    }
}
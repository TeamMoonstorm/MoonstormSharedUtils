using MSU;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExampleMod
{
    public class ExampleContent : IContentPackProvider
    {
        public string identifier => ExampleMain.GUID;
        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(exampleContentPack);
        internal static ContentPack exampleContentPack { get; } = new ContentPack();

        internal static ParallelMultiStartCoroutine _parallelPreLoadDispatchers = new ParallelMultiStartCoroutine();
        private static Func<IEnumerator>[] _loadDispatchers;
        internal static ParallelMultiStartCoroutine _parallelPostLoadDispatchers = new ParallelMultiStartCoroutine();

        private static Action[] _fieldAssignDispatchers;

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var enumerator = ExampleAssets.Initialize();
            while (enumerator.MoveNext())
                yield return null;

            _parallelPreLoadDispatchers.Start();
            while (!_parallelPreLoadDispatchers.IsDone()) yield return null;

            for (int i = 0; i < _loadDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _loadDispatchers.Length, 0.1f, 0.2f));
                enumerator = _loadDispatchers[i]();

                while (enumerator?.MoveNext() ?? false) yield return null;
            }

            _parallelPostLoadDispatchers.Start();
            while (!_parallelPostLoadDispatchers.IsDone) yield return null;

            for (int i = 0; i < _fieldAssignDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _fieldAssignDispatchers.Length, 0.95f, 0.99f));
                _fieldAssignDispatchers[i]();
            }
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(exampleContentPack, args.output);
            args.ReportProgress(1f);
            yield return null;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        private static IEnumerator LoadFromAssetBundles()
        {
            yield break;
        }

        private IEnumerator AddExampleExpansionDef()
        {
            yield break;
        }

        internal ExampleContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
            ExampleAssets._onExampleAssetsInitialized += () =>
            {
                _parallelPreLoadDispatchers.Add(AddExampleExpansionDef);
            };
        }

        static ExampleContent()
        {
            ExampleMain main = ExampleMain.instance;
            _loadDispatchers = new Func<IEnumerator>[]
            {
                () =>
                {
                    ItemModule.AddProvider(main, ContentUtil.CreateContentPieceProvider<ItemDef>(main, exampleContentPack));
                    return ItemModule.InitializeItems(main);
                },
                () =>
                {
                    EquipmentModule.AddProvider(main, ContentUtil.CreateContentPieceProvider<EquipmentDef>(main, exampleContentPack));
                    return EquipmentModule.InitialzeEquipments(main);
                },
                LoadFromAssetBundles
            };

            _fieldAssignDispatchers = new Action[]
            {
                () => ContentUtil.PopulateTypeFields(typeof(Items), exampleContentPack.itemDefs),
                () => ContentUtil.PopulateTypeFields(typeof(Equipments), exampleContentPack.equipmentDefs)
            };
        }

        public static class Items
        {
            public static ItemDef ExampleItem;
        }

        public static class Equipments
        {
            public static EquipmentDef ExampleEquipment;
        }
    }
}
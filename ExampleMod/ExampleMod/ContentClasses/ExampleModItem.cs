using MSU;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2.ContentManagement;

namespace ExampleMod
{
    /// <summary>
    /// <inheritdoc cref="IItemContentPiece"/>
    /// </summary>
    public abstract class ExampleModItem : IItemContentPiece, IContentPackModifier
    {
        public ItemAssetCollection AssetCollection { get; private set; }
        public NullableRef<List<GameObject>> ItemDisplayPrefabs { get; protected set; } = new List<GameObject>();
        public ItemDef ItemDef { get; protected set; }

        ItemDef IContentPiece<ItemDef>.Asset => ItemDef;
        NullableRef<List<GameObject>> IItemContentPiece.ItemDisplayPrefabs => ItemDisplayPrefabs;

        public abstract ExampleAssetRequest AssetRequest { get; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            ExampleAssetRequest request = AssetRequest;

            request.StartLoad();
            while (!request.isComplete)
                yield return null;

            if(request.boxedAsset is ItemAssetCollection collection)
            {
                AssetCollection = collection;

                ItemDef = AssetCollection.itemDef;
                ItemDisplayPrefabs = AssetCollection.itemDisplayPrefabs;
            }
            else if(request.boxedAsset is ItemDef def)
            {
                ItemDef = def;
            }
            else
            {
                ExampleLog.Error("Invalid AssetRequest " + request.assetName + " of type " + request.boxedAsset.GetType());
            }
        }

        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            if(AssetCollection)
                contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}
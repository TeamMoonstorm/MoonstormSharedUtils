using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R2API.ScriptableObjects;
using System.Collections.ObjectModel;
using System;
using System.Linq;

namespace Moonstorm
{
    public abstract class ContentModule<T> : ModuleBase<T> where T : ContentBase
    {
        public abstract R2APISerializableContentPack SerializableContentPack { get; }

        protected bool AddSafely<TAsset>(ref TAsset[] contentPackArray, TAsset content) where TAsset : UnityEngine.Object
        {
            try
            { 
                if(contentPackArray.Contains(content))
                {
                    throw new InvalidOperationException($"Cannot add {content} to {SerializableContentPack} because the asset has already been added to it's corresponding array!");
                }
                HG.ArrayUtils.ArrayAppend(ref contentPackArray, content);
                return true;
            }
            catch(Exception e) 
            {
                MSULog.Error($"{e} (Content: {content})");
                if (contentPackArray.Contains(content))
                    return true;
                return false;
            }
        }
    }
}
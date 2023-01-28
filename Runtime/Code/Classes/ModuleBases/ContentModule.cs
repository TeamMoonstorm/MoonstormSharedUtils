using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R2API.ScriptableObjects;
using System.Collections.ObjectModel;
using System;
using System.Linq;

namespace Moonstorm
{
    /// <summary>
    /// A class that extends ModuleBase.
    /// <para>A ContentModule contains extra methods and an abstract property for a <see cref="R2APISerializableContentPack"/> to add specific content pieces to your mod's SerializableContentPack</para>
    /// </summary>
    /// <typeparam name="T">The type of content base this module base manages</typeparam>
    public abstract class ContentModule<T> : ModuleBase<T> where T : ContentBase
    {
        /// <summary>
        /// Your mod's SerializableContentPack
        /// </summary>
        public abstract R2APISerializableContentPack SerializableContentPack { get; }

        /// <summary>
        /// The add safely method will add a piece of <paramref name="content"/> of type <typeparamref name="TAsset"/> to the array in <paramref name="contentPackArray"/>
        /// The method does not add the content piece to the array if its already in said array
        /// </summary>
        /// <typeparam name="TAsset">The type of content piece to add, Ex: <see cref="RoR2.ItemDef"/></typeparam>
        /// <param name="contentPackArray">The array of the SerializableContentPack to add to</param>
        /// <param name="content">The piece of content being added</param>
        /// <param name="correspondingArrayName">a specification of the <paramref name="contentPackArray"/>'s content type array, useful if youre adding content pieces to arrays of type GameObject and you want to specify the main component type (ex: <see cref="RoR2.CharacterBody"/></param>
        /// <returns>True if added succesfully, false otherwise</returns>
        protected bool AddSafely<TAsset>(ref TAsset[] contentPackArray, TAsset content, string correspondingArrayName = null) where TAsset : UnityEngine.Object
        {
            if (contentPackArray.Contains(content)) //Content already in the contentPack for whatever reason? return true;
            {
#if DEBUG
                MSULog.Warning($"Content {content} was already in {SerializableContentPack}'s {correspondingArrayName ?? content.GetType().Name} array!\n" +
                    $"MSU automatically adds the content piece to its corresponding array in initialization, do not add it beforehand.");
#endif
                return true;
            }

            HG.ArrayUtils.ArrayAppend(ref contentPackArray, content);
            return true;
        }
    }
}
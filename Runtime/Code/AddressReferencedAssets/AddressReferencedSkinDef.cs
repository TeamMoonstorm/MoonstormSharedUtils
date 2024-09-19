﻿using R2API.AddressReferencedAssets;
using RoR2;
using System;

namespace MSU.AddressReferencedAssets
{
    /// <summary>
    /// A type of AddressReferencedAsset used to reference SkinDefs.
    /// </summary>
    [Serializable]
    public class AddressReferencedSkinDef : AddressReferencedAsset<SkinDef>
    {
        /// <summary>
        /// Operator for casting <see cref="AddressReferencedExpansionDef"/> to a boolean value
        /// <br>Allows you to keep using the unity Syntax for checking if an object exists.</br>
        /// </summary>
        public static implicit operator bool(AddressReferencedSkinDef addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        /// <summary>
        /// Operator for casting <see cref="AddressReferencedExpansionDef"/> to it's currently loaded <see cref="Asset"/> value
        /// </summary>
        public static implicit operator SkinDef(AddressReferencedSkinDef addressReferencedAsset)
        {
            return addressReferencedAsset?.Asset;
        }

        /// <summary>
        /// Operator for encapsulating a <see cref="string"/> inside an <see cref="AddressReferencedSkinDef"/>
        /// </summary>
        public static implicit operator AddressReferencedSkinDef(string address)
        {
            return new AddressReferencedSkinDef(address);
        }

        /// <summary>
        /// Operator for encapsulating an <see cref="SkinDef"/> inside an <see cref="AddressReferencedSkinDef"/>
        /// </summary>
        public static implicit operator AddressReferencedSkinDef(SkinDef asset)
        {
            return new AddressReferencedSkinDef(asset);
        }

        /// <summary>
        /// <inheritdoc cref="AddressReferencedAsset{T}.AddressReferencedAsset()"/>
        /// <br>T is <see cref="SkinDef"/></br>
        /// </summary>
        public AddressReferencedSkinDef() : base() { }

        /// <summary>
        /// <inheritdoc cref="AddressReferencedAsset{T}.AddressReferencedAsset(T)"/>
        /// <br>T is <see cref="SkinDef"/></br>
        /// </summary>
        public AddressReferencedSkinDef(SkinDef def) : base(def) { }

        /// <summary>
        /// <inheritdoc cref="AddressReferencedAsset{T}.AddressReferencedAsset(string)"/>
        /// <br>T is <see cref="SkinDef"/></br>
        /// </summary>
        public AddressReferencedSkinDef(string addressOrName) : base(addressOrName) { }
    }
}
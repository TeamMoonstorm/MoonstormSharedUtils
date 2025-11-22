using System;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// Used to display a string as a transform path
    /// </summary>
    public class TransformPathAttribute : PropertyAttribute
    {
        /// <summary>
        /// The Root Object from which the paths are calculated.
        /// <br></br>
        /// Supports both a GameObject, an AssetReferenceT{GameObject} and a AdressReferencedPrefab
        /// </summary>
        public string rootObjectProperty { get; }

        /// <summary>
        /// The name of a sibling property that can be used to inferr a required type within the RootTransform's childrens
        /// </summary>
        public string siblingPropertyComponentTypeRequirement { get; set; }

        /// <summary>
        /// If specified, the RootTransform will be the _first_ object where this component is found.
        /// </summary>
        public Type rootComponentType { get; set; }

        /// <summary>
        /// If true, the RootObject can be selected
        /// </summary>
        public bool allowSelectingRoot { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rootObjectProperty">assigns <see cref="rootObjectProperty"/></param>
        public TransformPathAttribute(string rootObjectProperty)
        {
            this.rootObjectProperty = rootObjectProperty;
        }
    }
}
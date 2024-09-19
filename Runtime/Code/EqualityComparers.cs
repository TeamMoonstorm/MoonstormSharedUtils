using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MSU
{
    /// <summary>
    /// An Equality comparer for <see cref="CharacterBody"/> which compares the equality of each body's <see cref="BodyIndex"/>
    /// </summary>
    public struct CharacterBodyIndexEqualityComparer : IEqualityComparer<CharacterBody>
    {
        /// <summary>
        /// Checks if two body's indices are equal.
        /// </summary>
        public bool Equals(CharacterBody x, CharacterBody y)
        {
            if (!x || !y)
                return false;

            if (x.bodyIndex == BodyIndex.None || y.bodyIndex == BodyIndex.None)
                return false;

            return x.bodyIndex == y.bodyIndex;
        }
        /// <summary>
        /// Obtains the HashCode for a given CharacterBody
        /// </summary>
        public int GetHashCode(CharacterBody obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// An Equality comparer for an Interactable which compares the equality of each Interactable's NetworkIdentity's assetID
    /// </summary>
    public struct IInteractableNetworkIdentityAssetIDComparer : IEqualityComparer<IInteractable>
    {
        /// <summary>
        /// Compares if two interactable's <see cref="NetworkIdentity.assetId"/> are equal
        /// </summary>
        public bool Equals(IInteractable x, IInteractable y)
        {
            if (x == null || y == null)
                return false;

            MonoBehaviour xAsBehaviour = x as MonoBehaviour;
            MonoBehaviour yAsBehaviour = y as MonoBehaviour;

            if (!xAsBehaviour || !yAsBehaviour)
                return false;

            var xNetID = xAsBehaviour.GetComponent<NetworkIdentity>();
            var yNetID = yAsBehaviour.GetComponent<NetworkIdentity>();

            if (!xNetID || !yNetID)
                return false;

            return xNetID.assetId.Equals(yNetID.assetId);
        }

        /// <summary>
        /// Obtains the HashCode for a given <see cref="IInteractable"/>
        /// </summary>
        public int GetHashCode(IInteractable obj)
        {
            return obj?.GetHashCode() ?? -1;
        }
    }
}
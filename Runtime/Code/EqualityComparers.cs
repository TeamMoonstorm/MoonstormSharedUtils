using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MSU
{
    /// <summary>
    /// An Equality comparer for <see cref="CharacterBody"/> which compares the equality of each body's <see cref="BodyIndex"/>
    /// </summary>
    public struct CharacterBodyIndexEqualityComparer : IEqualityComparer<CharacterBody>
    {
        public bool Equals(CharacterBody x, CharacterBody y)
        {
            if (!x || !y)
                return false;

            if (x.bodyIndex == BodyIndex.None || y.bodyIndex == BodyIndex.None)
                return false;

            return x.bodyIndex == y.bodyIndex;
        }

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

        public int GetHashCode(IInteractable obj)
        {
            return obj?.GetHashCode() ?? -1;
        }
    }
}
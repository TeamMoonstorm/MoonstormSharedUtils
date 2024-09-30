using RoR2;
using System;
using System.Collections;
using UnityEngine;

namespace Moonstorm.Components
{
    [Obsolete]
    public class MoonstormContentManager : MonoBehaviour
    {
        public bool hasMaster;

        [SerializeField]
        public CharacterBody body;

        public MoonstormEliteBehavior eliteBehavior;

        private void Start()
        {
            throw new System.NotImplementedException();
        }

        public void CheckItemEquipments()
        {
            throw new System.NotImplementedException();
        }

        public void StartGetInterfaces() => StartCoroutine(GetInterfaces());

        private IEnumerator GetInterfaces()
        {
            throw new System.NotImplementedException();
        }

        private void CheckEliteBehavior()
        {
            throw new System.NotImplementedException();
        }

        public void RunStatRecalculationsStart()
        {
            throw new System.NotImplementedException();
        }

        public void RunStatRecalculationsEnd()
        {
            throw new System.NotImplementedException();
        }

        public void RunStatHookEventModifiers(R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            throw new System.NotImplementedException();
        }
        private void OnDestroy()
        {
            throw new System.NotImplementedException();
        }
    }
}

using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSU
{
    public class GenericRendererInfoContainer : MonoBehaviour
    {
        public CharacterModel.RendererInfo[] rendererInfos = Array.Empty<CharacterModel.RendererInfo>();

        [ContextMenu("Apply Infos")]
        private void ApplyInfos()
        {
            for(int i = 0; i < rendererInfos.Length; i++)
            {
                var rendererInfo = rendererInfos[i];
                var renderer = rendererInfo.renderer;

                renderer.shadowCastingMode = rendererInfo.defaultShadowCastingMode;
                renderer.sharedMaterial = rendererInfo.defaultMaterial;
            }
        }

        [ContextMenu("Get Infos from Children")]
        private void GetInfosFromChildren()
        {
            Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
            for(int i = 0; i < childRenderers.Length; i++)
            {
                var renderer = childRenderers[i];
                bool alreadyDefined = false;
                for(int j = 0; j < rendererInfos.Length; j++)
                {
                    if (rendererInfos[j].renderer == renderer)
                    {
                        alreadyDefined = true;
                        break;
                    }
                }

                if(alreadyDefined)
                {
                    break;
                }

                HG.ArrayUtils.ArrayAppend(ref rendererInfos, new CharacterModel.RendererInfo
                {
                    renderer = renderer,
                    defaultMaterial = renderer.sharedMaterial,
                    defaultShadowCastingMode = renderer.shadowCastingMode,
                });
            }
        }
    }
}

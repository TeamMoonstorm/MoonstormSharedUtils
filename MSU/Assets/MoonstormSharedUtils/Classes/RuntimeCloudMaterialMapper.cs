using System;
using UnityEngine;

namespace Moonstorm
{
    internal class RuntimeCloudMaterialMapper
    {
        public string[] shaderKeywords;
        private static string[] keywordStrings = new string[]
        {
            "DISABLEREMAP",
            "USE_UV1",
            "FADECLOSE",
            "USE_CLOUDS",
            "CLOUDOFFSET",
            "VERTEXCOLOR",
            "VERTEXALPHA",
            "CALCTEXTUREALPHA",
            "VERTEXOFFSET",
            "FRESNEL",
            "SKYBOX_ONLY"
        };

        public Color _Tint = Color.white;
        public Texture _MainTex;
        public Vector2 _MainTexScale = Vector2.one;
        public Vector2 _MainTexOffset = Vector2.zero;
        public Texture _RemapTex;
        public Vector2 _RemapTexScale = Vector2.one;
        public Vector2 _RemapTexOffset = Vector2.zero;

        [Range(0f, 2f)]
        public float _InvFade = 0.1f;

        [Range(1f, 20f)]
        public float _BrightnessBoost = 1f;

        [Range(0f, 20f)]
        public float _AlphaBoost = 1f;

        [Range(0f, 1f)]
        public float _AlphaBias = 0;

        [Range(0f, 1f)]
        public float _FadeCloseDistance = 0.5f;

        public enum _CullEnum
        {
            Off,
            Front,
            Back
        }
        public _CullEnum _Cull_Mode;

        public enum _ZTestEnum
        {
            Disabled,
            Never,
            Less,
            Equal,
            LessEqual,
            Greater,
            NotEqual,
            GreaterEqual,
            Always
        }
        public _ZTestEnum _ZTest_Mode = _ZTestEnum.LessEqual;

        [Range(-10f, 10f)]
        public float _DepthOffset;


        [Range(-2f, 2f)]
        public float _DistortionStrength = 0.1f;

        public Texture _Cloud1Tex;
        public Vector2 _Cloud1TexScale;
        public Vector2 _Cloud1TexOffset;
        public Texture _Cloud2Tex;
        public Vector2 _Cloud2TexScale;
        public Vector2 _Cloud2TexOffset;
        public Vector4 _CutoffScroll;

        [Range(-20f, 20f)]
        public float _FresnelPower;

        [Range(0f, 3f)]
        public float _VertexOffsetAmount;

        public RuntimeCloudMaterialMapper(Material material)
        {
            GetMaterialValues(material);
        }

        public void GetMaterialValues(Material material)
        {
            shaderKeywords = material.shaderKeywords;
            _Tint = material.GetColor("_TintColor");
            _MainTex = material.GetTexture("_MainTex");
            _MainTexScale = material.GetTextureScale("_MainTex");
            _MainTexOffset = material.GetTextureOffset("_MainTex");
            _RemapTex = material.GetTexture("_RemapTex");
            _RemapTexScale = material.GetTextureScale("_RemapTex");
            _RemapTexOffset = material.GetTextureOffset("_RemapTex");
            _InvFade = material.GetFloat("_InvFade");
            _BrightnessBoost = material.GetFloat("_Boost");
            _AlphaBoost = material.GetFloat("_AlphaBoost");
            _AlphaBias = material.GetFloat("_AlphaBias");
            _FadeCloseDistance = material.GetFloat("_FadeCloseDistance");
            _Cull_Mode = (_CullEnum)(int)material.GetFloat("_Cull");
            _ZTest_Mode = (_ZTestEnum)(int)material.GetFloat("_ZTest");
            _DepthOffset = material.GetFloat("_DepthOffset");
            _DistortionStrength = material.GetFloat("_DistortionStrength");
            _Cloud1Tex = material.GetTexture("_Cloud1Tex");
            _Cloud1TexScale = material.GetTextureScale("_Cloud1Tex");
            _Cloud1TexOffset = material.GetTextureOffset("_Cloud1Tex");
            _Cloud2Tex = material.GetTexture("_Cloud2Tex");
            _Cloud2TexScale = material.GetTextureScale("_Cloud2Tex");
            _Cloud2TexOffset = material.GetTextureOffset("_Cloud2Tex");
            _CutoffScroll = material.GetVector("_CutoffScroll");
            _FresnelPower = material.GetFloat("_FresnelPower");
            _VertexOffsetAmount = material.GetFloat("_OffsetAmount");
        }

        public void SetMaterialValues(ref Material material)
        {
            foreach (var keyword in keywordStrings)
                if (material.IsKeywordEnabled(keyword))
                    material.DisableKeyword(keyword);
            foreach (var keyword in shaderKeywords)
                material.EnableKeyword(keyword);

            material.SetColor("_TintColor", _Tint);

            if (_MainTex)
            {
                material.SetTexture("_MainTex", _MainTex);
                material.SetTextureScale("_MainTex", _MainTexScale);
                material.SetTextureOffset("_MainTex", _MainTexOffset);
            }
            else
            {
                material.SetTexture("_MainTex", null);
            }

            if (_RemapTex)
            {
                material.SetTexture("_RemapTex", _RemapTex);
                material.SetTextureScale("_RemapTex", _RemapTexScale);
                material.SetTextureOffset("_RemapTex", _RemapTexOffset);
            }
            else
            {
                material.SetTexture("_RemapTex", null);
            }

            material.SetFloat("_InvFade", _InvFade);
            material.SetFloat("_Boost", _BrightnessBoost);
            material.SetFloat("_AlphaBoost", _AlphaBoost);
            material.SetFloat("_AlphaBias", _AlphaBias);
            material.SetFloat("_FadeCloseDistance", _FadeCloseDistance);
            material.SetFloat("_Cull", Convert.ToSingle(_Cull_Mode));
            material.SetFloat("_ZTest", Convert.ToSingle(_ZTest_Mode));
            material.SetFloat("_DepthOffset", _DepthOffset);
            material.SetFloat("_DistortionStrength", _DistortionStrength);

            if (_Cloud1Tex)
            {
                material.SetTexture("_Cloud1Tex", _Cloud1Tex);
                material.SetTextureScale("_Cloud1Tex", _Cloud1TexScale);
                material.SetTextureOffset("_Cloud1Tex", _Cloud1TexOffset);
            }
            else
            {
                material.SetTexture("_Cloud1Tex", null);
            }

            if (_Cloud2Tex)
            {
                material.SetTexture("_Cloud2Tex", _Cloud2Tex);
                material.SetTextureScale("_Cloud2Tex", _Cloud2TexScale);
                material.SetTextureOffset("_Cloud2Tex", _Cloud2TexOffset);
            }
            else
            {
                material.SetTexture("_Cloud2Tex", null);
            }

            material.SetVector("_CutoffScroll", _CutoffScroll);
            material.SetFloat("_FresnelPower", _FresnelPower);
            material.SetFloat("_OffsetAmount", _VertexOffsetAmount);


        }
    }
}

#if DEBUG
using System;
using UnityEngine;

namespace Moonstorm.Components
{
    public class MaterialControllerComponents
    {
        public static void SetShaderKeywordBasedOnBool(bool enabled, Material material, string keyword)
        {
            if (!material)
            {
                MSULog.Error($"Material field was null, cannot run shader keyword method.");
                return;
            }

            if (enabled)
            {
                if (!material.IsKeywordEnabled(keyword))
                {
                    material.EnableKeyword(keyword);
                }
            }
            else
            {
                if (material.IsKeywordEnabled(keyword))
                {
                    material.DisableKeyword(keyword);
                }
            }
        }

        public static void PutMaterialIntoMeshRenderer(Renderer meshRenderer, Material material)
        {
            if (material && meshRenderer)
            {
                if (meshRenderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    skinnedMeshRenderer.sharedMaterials = new Material[] { material };
                }

                if (meshRenderer is MeshRenderer meshRendererType)
                {
                    meshRendererType.material = material;
                }
            }
        }

        public class MaterialController : MonoBehaviour
        {
            public Material material;
            public Renderer renderer;
            public string MaterialName;
        }

        public class HGStandardController : MaterialController
        {

            public bool _EnableCutout;
            public Color _Color;
            public Texture _MainTex;
            public Vector2 _MainTexScale;
            public Vector2 _MainTexOffset;

            [Range(0f, 5f)]
            public float _NormalStrength;

            public Texture _NormalTex;
            public Vector2 _NormalTexScale;
            public Vector2 _NormalTexOffset;
            public Color _EmColor;
            public Texture _EmTex;

            [Range(0f, 10f)]
            public float _EmPower;

            [Range(0f, 1f)]
            public float _Smoothness;

            public bool _IgnoreDiffuseAlphaForSpeculars;

            public enum _RampInfoEnum
            {
                TwoTone = 0,
                SmoothedTwoTone = 1,
                Unlitish = 3,
                Subsurface = 4,
                Grass = 5
            }
            public _RampInfoEnum _RampChoice;

            public enum _DecalLayerEnum
            {
                Default = 0,
                Environment = 1,
                Character = 2,
                Misc = 3
            }
            public _DecalLayerEnum _DecalLayer;

            [Range(0f, 1f)]
            public float _SpecularStrength;

            [Range(0.1f, 20f)]
            public float _SpecularExponent;

            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull_Mode;

            public bool _EnableDither;

            [Range(0f, 1f)]
            public float _FadeBias;

            public bool _EnableFresnelEmission;

            public Texture _FresnelRamp;

            [Range(0.1f, 20f)]
            public float _FresnelPower;

            public Texture _FresnelMask;

            [Range(0f, 20f)]
            public float _FresnelBoost;

            public bool _EnablePrinting;

            [Range(-25f, 25f)]
            public float _SliceHeight;

            [Range(0f, 10f)]
            public float _PrintBandHeight;

            [Range(0f, 1f)]
            public float _PrintAlphaDepth;

            public Texture _PrintAlphaTexture;
            public Vector2 _PrintAlphaTextureScale;
            public Vector2 _PrintAlphaTextureOffset;

            [Range(0f, 10f)]
            public float _PrintColorBoost;

            [Range(0f, 4f)]
            public float _PrintAlphaBias;

            [Range(0f, 1f)]
            public float _PrintEmissionToAlbedoLerp;

            public enum _PrintDirectionEnum
            {
                BottomUp = 0,
                TopDown = 1,
                BackToFront = 3
            }
            public _PrintDirectionEnum _PrintDirection;

            public Texture _PrintRamp;

            [Range(-10f, 10f)]
            public float _EliteBrightnessMin;

            [Range(-10f, 10f)]
            public float _EliteBrightnessMax;

            public bool _EnableSplatmap;
            public bool _UseVertexColorsInstead;

            [Range(0f, 1f)]
            public float _BlendDepth;

            public Texture _SplatmapTex;
            public Vector2 _SplatmapTexScale;
            public Vector2 _SplatmapTexOffset;

            [Range(0f, 20f)]
            public float _SplatmapTileScale;

            public Texture _GreenChannelTex;
            public Texture _GreenChannelNormalTex;

            [Range(0f, 1f)]
            public float _GreenChannelSmoothness;

            [Range(-2f, 5f)]
            public float _GreenChannelBias;

            public Texture _BlueChannelTex;
            public Texture _BlueChannelNormalTex;

            [Range(0f, 1f)]
            public float _BlueChannelSmoothness;

            [Range(-2f, 5f)]
            public float _BlueChannelBias;

            public bool _EnableFlowmap;
            public Texture _FlowTexture;
            public Texture _FlowHeightmap;
            public Vector2 _FlowHeightmapScale;
            public Vector2 _FlowHeightmapOffset;
            public Texture _FlowHeightRamp;
            public Vector2 _FlowHeightRampScale;
            public Vector2 _FlowHeightRampOffset;

            [Range(-1f, 1f)]
            public float _FlowHeightBias;

            [Range(0.1f, 20f)]
            public float _FlowHeightPower;

            [Range(0.1f, 20f)]
            public float _FlowEmissionStrength;

            [Range(0f, 15f)]
            public float _FlowSpeed;

            [Range(0f, 5f)]
            public float _MaskFlowStrength;

            [Range(0f, 5f)]
            public float _NormalFlowStrength;

            [Range(0f, 10f)]
            public float _FlowTextureScaleFactor;

            public bool _EnableLimbRemoval;

            public void Start()
            {
                GrabMaterialValues();
            }
            public void GrabMaterialValues()
            {
                if (material)
                {
                    _EnableCutout = material.IsKeywordEnabled("CUTOUT");
                    _Color = material.GetColor("_Color");
                    _MainTex = material.GetTexture("_MainTex");
                    _MainTexScale = material.GetTextureScale("_MainTex");
                    _MainTexOffset = material.GetTextureOffset("_MainTex");
                    _NormalStrength = material.GetFloat("_NormalStrength");
                    _NormalTex = material.GetTexture("_NormalTex");
                    _NormalTexScale = material.GetTextureScale("_NormalTex");
                    _NormalTexOffset = material.GetTextureOffset("_NormalTex");
                    _EmColor = material.GetColor("_EmColor");
                    _EmTex = material.GetTexture("_EmTex");
                    _EmPower = material.GetFloat("_EmPower");
                    _Smoothness = material.GetFloat("_Smoothness");
                    _IgnoreDiffuseAlphaForSpeculars = material.IsKeywordEnabled("FORCE_SPEC");
                    _RampChoice = (_RampInfoEnum)(int)material.GetFloat("_RampInfo");
                    _DecalLayer = (_DecalLayerEnum)(int)material.GetFloat("_DecalLayer");
                    _SpecularStrength = material.GetFloat("_SpecularStrength");
                    _SpecularExponent = material.GetFloat("_SpecularExponent");
                    _Cull_Mode = (_CullEnum)(int)material.GetFloat("_Cull");
                    _EnableDither = material.IsKeywordEnabled("DITHER");
                    _FadeBias = material.GetFloat("_FadeBias");
                    _EnableFresnelEmission = material.IsKeywordEnabled("FRESNEL_EMISSION");
                    _FresnelRamp = material.GetTexture("_FresnelRamp");
                    _FresnelPower = material.GetFloat("_FresnelPower");
                    _FresnelMask = material.GetTexture("_FresnelMask");
                    _FresnelBoost = material.GetFloat("_FresnelBoost");
                    _EnablePrinting = material.IsKeywordEnabled("PRINT_CUTOFF");
                    _SliceHeight = material.GetFloat("_SliceHeight");
                    _PrintBandHeight = material.GetFloat("_SliceBandHeight");
                    _PrintAlphaDepth = material.GetFloat("_SliceAlphaDepth");
                    _PrintAlphaTexture = material.GetTexture("_SliceAlphaTex");
                    _PrintAlphaTextureScale = material.GetTextureScale("_SliceAlphaTex");
                    _PrintAlphaTextureOffset = material.GetTextureOffset("_SliceAlphaTex");
                    _PrintColorBoost = material.GetFloat("_PrintBoost");
                    _PrintAlphaBias = material.GetFloat("_PrintBias");
                    _PrintEmissionToAlbedoLerp = material.GetFloat("_PrintEmissionToAlbedoLerp");
                    _PrintDirection = (_PrintDirectionEnum)(int)material.GetFloat("_PrintDirection");
                    _PrintRamp = material.GetTexture("_PrintRamp");
                    _EliteBrightnessMin = material.GetFloat("_EliteBrightnessMin");
                    _EliteBrightnessMax = material.GetFloat("_EliteBrightnessMax");
                    _EnableSplatmap = material.IsKeywordEnabled("SPLATMAP");
                    _UseVertexColorsInstead = material.IsKeywordEnabled("USE_VERTEX_COLORS");
                    _BlendDepth = material.GetFloat("_Depth");
                    _SplatmapTex = material.GetTexture("_SplatmapTex");
                    _SplatmapTexScale = material.GetTextureScale("_SplatmapTex");
                    _SplatmapTexOffset = material.GetTextureOffset("_SplatmapTex");
                    _SplatmapTileScale = material.GetFloat("_SplatmapTileScale");
                    _GreenChannelTex = material.GetTexture("_GreenChannelTex");
                    _GreenChannelNormalTex = material.GetTexture("_GreenChannelNormalTex");
                    _GreenChannelSmoothness = material.GetFloat("_GreenChannelSmoothness");
                    _GreenChannelBias = material.GetFloat("_GreenChannelBias");
                    _BlueChannelTex = material.GetTexture("_BlueChannelTex");
                    _BlueChannelNormalTex = material.GetTexture("_BlueChannelNormalTex");
                    _BlueChannelSmoothness = material.GetFloat("_BlueChannelSmoothness");
                    _BlueChannelBias = material.GetFloat("_BlueChannelBias");
                    _EnableFlowmap = material.IsKeywordEnabled("FLOWMAP");
                    _FlowTexture = material.GetTexture("_FlowTex");
                    _FlowHeightmap = material.GetTexture("_FlowHeightmap");
                    _FlowHeightmapScale = material.GetTextureScale("_FlowHeightmap");
                    _FlowHeightmapOffset = material.GetTextureOffset("_FlowHeightmap");
                    _FlowHeightRamp = material.GetTexture("_FlowHeightRamp");
                    _FlowHeightRampScale = material.GetTextureScale("_FlowHeightRamp");
                    _FlowHeightRampOffset = material.GetTextureOffset("_FlowHeightRamp");
                    _FlowHeightBias = material.GetFloat("_FlowHeightBias");
                    _FlowHeightPower = material.GetFloat("_FlowHeightPower");
                    _FlowEmissionStrength = material.GetFloat("_FlowEmissionStrength");
                    _FlowSpeed = material.GetFloat("_FlowSpeed");
                    _MaskFlowStrength = material.GetFloat("_FlowMaskStrength");
                    _NormalFlowStrength = material.GetFloat("_FlowNormalStrength");
                    _FlowTextureScaleFactor = material.GetFloat("_FlowTextureScaleFactor");
                    _EnableLimbRemoval = material.IsKeywordEnabled("LIMBREMOVAL");
                    MaterialName = material.name;
                }
            }

            public void Update()
            {
                if (material)
                {
                    if (material.name != MaterialName && renderer)
                    {
                        GrabMaterialValues();
                        PutMaterialIntoMeshRenderer(renderer, material);
                    }

                    SetShaderKeywordBasedOnBool(_EnableCutout, material, "CUTOUT");

                    material.SetColor("_Color", _Color);

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

                    material.SetFloat("_NormalStrength", _NormalStrength);

                    if (_NormalTex)
                    {
                        material.SetTexture("_NormalTex", _NormalTex);
                        material.SetTextureScale("_NormalTex", _NormalTexScale);
                        material.SetTextureOffset("_NormalTex", _NormalTexOffset);
                    }
                    else
                    {
                        material.SetTexture("_NormalTex", null);
                    }

                    material.SetColor("_EmColor", _EmColor);

                    if (_EmTex)
                    {
                        material.SetTexture("_EmTex", _EmTex);
                    }
                    else
                    {
                        material.SetTexture("_EmTex", null);
                    }

                    material.SetFloat("_EmPower", _EmPower);
                    material.SetFloat("_Smoothness", _Smoothness);

                    SetShaderKeywordBasedOnBool(_IgnoreDiffuseAlphaForSpeculars, material, "FORCE_SPEC");

                    material.SetFloat("_RampInfo", Convert.ToSingle(_RampChoice));
                    material.SetFloat("_DecalLayer", Convert.ToSingle(_DecalLayer));
                    material.SetFloat("_SpecularStrength", _SpecularStrength);
                    material.SetFloat("_SpecularExponent", _SpecularExponent);
                    material.SetFloat("_Cull", Convert.ToSingle(_Cull_Mode));

                    SetShaderKeywordBasedOnBool(_EnableDither, material, "DITHER");

                    material.SetFloat("_FadeBias", _FadeBias);

                    SetShaderKeywordBasedOnBool(_EnableFresnelEmission, material, "FRESNEL_EMISSION");

                    if (_FresnelRamp)
                    {
                        material.SetTexture("_FresnelRamp", _FresnelRamp);
                    }
                    else
                    {
                        material.SetTexture("_FresnelRamp", null);
                    }

                    material.SetFloat("_FresnelPower", _FresnelPower);

                    if (_FresnelMask)
                    {
                        material.SetTexture("_FresnelMask", _FresnelMask);
                    }
                    else
                    {
                        material.SetTexture("_FresnelMask", null);
                    }

                    material.SetFloat("_FresnelBoost", _FresnelBoost);

                    SetShaderKeywordBasedOnBool(_EnablePrinting, material, "PRINT_CUTOFF");

                    material.SetFloat("_SliceHeight", _SliceHeight);
                    material.SetFloat("_SliceBandHeight", _PrintBandHeight);
                    material.SetFloat("_SliceAlphaDepth", _PrintAlphaDepth);

                    if (_PrintAlphaTexture)
                    {
                        material.SetTexture("_SliceAlphaTex", _PrintAlphaTexture);
                        material.SetTextureScale("_SliceAlphaTex", _PrintAlphaTextureScale);
                        material.SetTextureOffset("_SliceAlphaTex", _PrintAlphaTextureOffset);
                    }
                    else
                    {
                        material.SetTexture("_SliceAlphaTex", null);
                    }

                    material.SetFloat("_PrintBoost", _PrintColorBoost);
                    material.SetFloat("_PrintBias", _PrintAlphaBias);
                    material.SetFloat("_PrintEmissionToAlbedoLerp", _PrintEmissionToAlbedoLerp);
                    material.SetFloat("_PrintDirection", Convert.ToSingle(_PrintDirection));

                    if (_PrintRamp)
                    {
                        material.SetTexture("_PrintRamp", _PrintRamp);
                    }
                    else
                    {
                        material.SetTexture("_PrintRamp", null);
                    }

                    material.SetFloat("_EliteBrightnessMin", _EliteBrightnessMin);
                    material.SetFloat("_EliteBrightnessMax", _EliteBrightnessMax);

                    SetShaderKeywordBasedOnBool(_EnableSplatmap, material, "SPLATMAP");
                    SetShaderKeywordBasedOnBool(_UseVertexColorsInstead, material, "USE_VERTEX_COLORS");

                    material.SetFloat("_Depth", _BlendDepth);

                    if (_SplatmapTex)
                    {
                        material.SetTexture("_SplatmapTex", _SplatmapTex);
                        material.SetTextureScale("_SplatmapTex", _SplatmapTexScale);
                        material.SetTextureOffset("_SplatmapTex", _SplatmapTexOffset);
                    }
                    else
                    {
                        material.SetTexture("_SplatmapTex", null);
                    }

                    material.SetFloat("_SplatmapTileScale", _SplatmapTileScale);

                    if (_GreenChannelTex)
                    {
                        material.SetTexture("_GreenChannelTex", _GreenChannelTex);
                    }
                    else
                    {
                        material.SetTexture("_GreenChannelTex", null);
                    }

                    if (_GreenChannelNormalTex)
                    {
                        material.SetTexture("_GreenChannelNormalTex", _GreenChannelNormalTex);
                    }
                    else
                    {
                        material.SetTexture("_GreenChannelNormalTex", null);
                    }

                    material.SetFloat("_GreenChannelSmoothness", _GreenChannelSmoothness);
                    material.SetFloat("_GreenChannelBias", _GreenChannelBias);

                    if (_BlueChannelTex)
                    {
                        material.SetTexture("_BlueChannelTex", _BlueChannelTex);
                    }
                    else
                    {
                        material.SetTexture("_BlueChannelTex", null);
                    }

                    if (_BlueChannelNormalTex)
                    {
                        material.SetTexture("_BlueChannelNormalTex", _BlueChannelNormalTex);
                    }
                    else
                    {
                        material.SetTexture("_BlueChannelNormalTex", null);
                    }

                    material.SetFloat("_BlueChannelSmoothness", _BlueChannelSmoothness);
                    material.SetFloat("_BlueChannelBias", _BlueChannelBias);

                    SetShaderKeywordBasedOnBool(_EnableFlowmap, material, "FLOWMAP");

                    if (_FlowTexture)
                    {
                        material.SetTexture("_FlowTex", _FlowTexture);
                    }
                    else
                    {
                        material.SetTexture("_FlowTex", null);
                    }

                    if (_FlowHeightmap)
                    {
                        material.SetTexture("_FlowHeightmap", _FlowHeightmap);
                        material.SetTextureScale("_FlowHeightmap", _FlowHeightmapScale);
                        material.SetTextureOffset("_FlowHeightmap", _FlowHeightmapOffset);
                    }
                    else
                    {
                        material.SetTexture("_FlowHeightmap", null);
                    }

                    if (_FlowHeightRamp)
                    {
                        material.SetTexture("_FlowHeightRamp", _FlowHeightRamp);
                        material.SetTextureScale("_FlowHeightRamp", _FlowHeightRampScale);
                        material.SetTextureOffset("_FlowHeightRamp", _FlowHeightRampOffset);
                    }
                    else
                    {
                        material.SetTexture("_FlowHeightRamp", null);
                    }

                    material.SetFloat("_FlowHeightBias", _FlowHeightBias);
                    material.SetFloat("_FlowHeightPower", _FlowHeightPower);
                    material.SetFloat("_FlowEmissionStrength", _FlowEmissionStrength);
                    material.SetFloat("_FlowSpeed", _FlowSpeed);
                    material.SetFloat("_FlowMaskStrength", _MaskFlowStrength);
                    material.SetFloat("_FlowNormalStrength", _NormalFlowStrength);
                    material.SetFloat("_FlowTextureScaleFactor", _FlowTextureScaleFactor);

                    SetShaderKeywordBasedOnBool(_EnableLimbRemoval, material, "LIMBREMOVAL");
                }
            }

        }

        public class HGSnowToppedController : MaterialController
        {
            public Color _Color;
            public Texture _MainTex;
            public Vector2 _MainTexScale;
            public Vector2 _MainTexOffset;

            [Range(0f, 5f)]
            public float _NormalStrength;

            public Texture _NormalTex;
            public Vector2 _NormalTexScale;
            public Vector2 _NormalTexOffset;

            public Texture _SnowTex;
            public Vector2 _SnowTexScale;
            public Vector2 _SnowTexOffset;

            public Texture _SnowNormalTex;
            public Vector2 _SnowNormalTexScale;
            public Vector2 _SnowNormalTexOffset;

            [Range(-1f, 1f)]
            public float _SnowBias;

            [Range(0f, 1f)]
            public float _Depth;

            public bool _IgnoreAlphaWeights;
            public bool _BlendWeightsBinarily;

            public enum _RampInfoEnum
            {
                TwoTone = 0,
                SmoothedTwoTone = 1,
                Unlitish = 3,
                Subsurface = 4,
                Grass = 5
            }
            public _RampInfoEnum _RampChoice;

            public bool _IgnoreDiffuseAlphaForSpeculars;

            [Range(0f, 1f)]
            public float _SpecularStrength;

            [Range(0.1f, 20f)]
            public float _SpecularExponent;

            [Range(0f, 1f)]
            public float _Smoothness;

            [Range(0f, 1f)]
            public float _SnowSpecularStrength;

            [Range(0.1f, 20f)]
            public float _SnowSpecularExponent;

            [Range(0f, 1f)]
            public float _SnowSmoothness;

            public bool _DitherOn;

            public bool _TriplanarOn;

            [Range(0f, 1f)]
            public float _TriplanarTextureFactor;

            public bool _SnowOn;

            public bool _GradientBiasOn;

            public Vector4 _GradientBiasVector;

            public bool __DirtOn;

            public Texture _DirtTex;
            public Vector2 _DirtTexScale;
            public Vector2 _DirtTexOffset;

            public Texture _DirtNormalTex;
            public Vector2 _DirtNormalTexScale;
            public Vector2 _DirtNormalTexOffset;

            [Range(-2f, 2f)]
            public float _DirtBias;

            [Range(0f, 1f)]
            public float _DirtSpecularStrength;

            [Range(0f, 20f)]
            public float _DirtSpecularExponent;

            [Range(0f, 1f)]
            public float _DirtSmoothness;

            public void Start()
            {
                GrabMaterialValues();
            }
            public void GrabMaterialValues()
            {
                if (material)
                {
                    _Color = material.GetColor("_Color");
                    _MainTex = material.GetTexture("_MainTex");
                    _MainTexScale = material.GetTextureScale("_MainTex");
                    _MainTexOffset = material.GetTextureOffset("_MainTex");
                    _NormalStrength = material.GetFloat("_NormalStrength");
                    _NormalTex = material.GetTexture("_NormalTex");
                    _NormalTexScale = material.GetTextureScale("_NormalTex");
                    _NormalTexOffset = material.GetTextureOffset("_NormalTex");
                    _SnowTex = material.GetTexture("_SnowTex");
                    _SnowTexScale = material.GetTextureScale("_SnowTex");
                    _SnowTexOffset = material.GetTextureOffset("_SnowTex");
                    _SnowNormalTex = material.GetTexture("_SnowNormalTex");
                    _SnowNormalTexScale = material.GetTextureScale("_SnowNormalTex");
                    _SnowNormalTexOffset = material.GetTextureOffset("_SnowNormalTex");
                    _SnowBias = material.GetFloat("_SnowBias");
                    _Depth = material.GetFloat("_Depth");
                    _IgnoreAlphaWeights = material.IsKeywordEnabled("IGNORE_BIAS");
                    _BlendWeightsBinarily = material.IsKeywordEnabled("BINARYBLEND");
                    _RampChoice = (_RampInfoEnum)(int)material.GetFloat("_RampInfo");
                    _IgnoreDiffuseAlphaForSpeculars = material.IsKeywordEnabled("FORCE_SPEC");
                    _SpecularStrength = material.GetFloat("_SpecularStrength");
                    _SpecularExponent = material.GetFloat("_SpecularExponent");
                    _Smoothness = material.GetFloat("_Smoothness");
                    _SnowSpecularStrength = material.GetFloat("_SnowSpecularStrength");
                    _SnowSpecularExponent = material.GetFloat("_SnowSpecularExponent");
                    _SnowSmoothness = material.GetFloat("_SnowSmoothness");
                    _DitherOn = material.IsKeywordEnabled("DITHER");
                    _TriplanarOn = material.IsKeywordEnabled("TRIPLANAR");
                    _TriplanarTextureFactor = material.GetFloat("_TriplanarTextureFactor");
                    _SnowOn = material.IsKeywordEnabled("MICROFACET_SNOW");
                    _GradientBiasOn = material.IsKeywordEnabled("GRADIENTBIAS");
                    _GradientBiasVector = material.GetVector("_GradientBiasVector");
                    __DirtOn = material.IsKeywordEnabled("DIRTON");
                    _DirtTex = material.GetTexture("_DirtTex");
                    _DirtTexScale = material.GetTextureScale("_DirtTex");
                    _DirtTexOffset = material.GetTextureOffset("_DirtTex");
                    _DirtNormalTex = material.GetTexture("_DirtNormalTex");
                    _DirtNormalTexScale = material.GetTextureScale("_DirtNormalTex");
                    _DirtNormalTexOffset = material.GetTextureOffset("_DirtNormalTex");
                    _DirtBias = material.GetFloat("_DirtBias");
                    _DirtSpecularStrength = material.GetFloat("_DirtSpecularStrength");
                    _DirtSpecularExponent = material.GetFloat("_DirtSpecularExponent");
                    _DirtSmoothness = material.GetFloat("_DirtSmoothness");
                    MaterialName = material.name;
                }
            }

            public void Update()
            {
                if (material)
                {
                    if (material.name != MaterialName && renderer)
                    {
                        GrabMaterialValues();
                        PutMaterialIntoMeshRenderer(renderer, material);
                    }

                    material.SetColor("_Color", _Color);

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

                    material.SetFloat("_NormalStrength", _NormalStrength);

                    if (_NormalTex)
                    {
                        material.SetTexture("_NormalTex", _NormalTex);
                        material.SetTextureScale("_NormalTex", _NormalTexScale);
                        material.SetTextureOffset("_NormalTex", _NormalTexOffset);
                    }
                    else
                    {
                        material.SetTexture("_NormalTex", null);
                    }

                    if (_SnowTex)
                    {
                        material.SetTexture("_SnowTex", _SnowTex);
                        material.SetTextureScale("_SnowTex", _SnowTexScale);
                        material.SetTextureOffset("_SnowTex", _SnowTexOffset);
                    }
                    else
                    {
                        material.SetTexture("_SnowTex", null);
                    }

                    if (_SnowNormalTex)
                    {
                        material.SetTexture("_SnowNormalTex", _SnowNormalTex);
                        material.SetTextureScale("_SnowNormalTex", _SnowNormalTexScale);
                        material.SetTextureOffset("_SnowNormalTex", _SnowNormalTexOffset);
                    }
                    else
                    {
                        material.SetTexture("_SnowNormalTex", null);
                    }

                    material.SetFloat("_SnowBias", _SnowBias);
                    material.SetFloat("_Depth", _Depth);

                    SetShaderKeywordBasedOnBool(_IgnoreAlphaWeights, material, "IGNORE_BIAS");
                    SetShaderKeywordBasedOnBool(_BlendWeightsBinarily, material, "BINARYBLEND");

                    material.SetFloat("_RampInfo", Convert.ToSingle(_RampChoice));

                    SetShaderKeywordBasedOnBool(_IgnoreDiffuseAlphaForSpeculars, material, "FORCE_SPEC");


                    material.SetFloat("_SpecularStrength", _SpecularStrength);
                    material.SetFloat("_SpecularExponent", _SpecularExponent);
                    material.SetFloat("_Smoothness", _Smoothness);

                    material.SetFloat("_SnowSpecularStrength", _SnowSpecularStrength);
                    material.SetFloat("_SnowSpecularExponent", _SnowSpecularExponent);
                    material.SetFloat("_SnowSmoothness", _SnowSmoothness);

                    SetShaderKeywordBasedOnBool(_DitherOn, material, "DITHER");

                    SetShaderKeywordBasedOnBool(_TriplanarOn, material, "TRIPLANAR");
                    material.SetFloat("_TriplanarTextureFactor", _TriplanarTextureFactor);
                    SetShaderKeywordBasedOnBool(_SnowOn, material, "MICROFACET_SNOW");

                    SetShaderKeywordBasedOnBool(_GradientBiasOn, material, "GRADIENTBIAS");
                    material.SetVector("_GradientBiasVector", _GradientBiasVector);
                    SetShaderKeywordBasedOnBool(__DirtOn, material, "DIRTON");

                    if (_DirtTex)
                    {
                        material.SetTexture("_DirtTex", _DirtTex);
                        material.SetTextureScale("_DirtTex", _DirtTexScale);
                        material.SetTextureOffset("_DirtTex", _DirtTexOffset);
                    }
                    else
                    {
                        material.SetTexture("_DirtTex", null);
                    }

                    if (_DirtNormalTex)
                    {
                        material.SetTexture("_DirtNormalTex", _DirtNormalTex);
                        material.SetTextureScale("_DirtNormalTex", _DirtNormalTexScale);
                        material.SetTextureOffset("_DirtNormalTex", _DirtNormalTexOffset);
                    }
                    else
                    {
                        material.SetTexture("_DirtNormalTex", null);
                    }

                    material.SetFloat("_DirtBias", _DirtBias);
                    material.SetFloat("_DirtSpecularStrength", _DirtSpecularStrength);
                    material.SetFloat("_DirtSpecularExponent", _DirtSpecularExponent);
                    material.SetFloat("_DirtSmoothness", _DirtSmoothness);
                }
            }

        }

        public class HGCloudRemapController : MaterialController
        {
            public enum _BlendEnums
            {
                Zero = 0,
                One = 1,
                DstColor = 2,
                SrcColor = 3,
                OneMinusDstColor = 4,
                SrcAlpha = 5,
                OneMinusSrcColor = 6,
                DstAlpha = 7,
                OneMinusDstAlpha = 8,
                SrcAlphaSaturate = 9,
                OneMinusSrcAlpha = 10
            }
            public _BlendEnums _SrcBlend;
            public _BlendEnums _DstBlend;

            public Color _Tint;
            public bool _DisableRemapping;
            public Texture _MainTex;
            public Vector2 _MainTexScale;
            public Vector2 _MainTexOffset;
            public Texture _RemapTex;
            public Vector2 _RemapTexScale;
            public Vector2 _RemapTexOffset;

            [Range(0f, 2f)]
            public float _SoftFactor;

            [Range(1f, 20f)]
            public float _BrightnessBoost;

            [Range(0f, 20f)]
            public float _AlphaBoost;

            [Range(0f, 1f)]
            public float _AlphaBias;

            public bool _UseUV1;
            public bool _FadeWhenNearCamera;

            [Range(0f, 1f)]
            public float _FadeCloseDistance;

            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull_Mode;

            public enum _ZTestEnum
            {
                Disabled = 0,
                Never = 1,
                Less = 2,
                Equal = 3,
                LessEqual = 4,
                Greater = 5,
                NotEqual = 6,
                GreaterEqual = 7,
                Always = 8
            }
            public _ZTestEnum _ZTest_Mode;

            [Range(-10f, 10f)]
            public float _DepthOffset;

            public bool _CloudRemapping;
            public bool _DistortionClouds;

            [Range(-2f, 2f)]
            public float _DistortionStrength;

            public Texture _Cloud1Tex;
            public Vector2 _Cloud1TexScale;
            public Vector2 _Cloud1TexOffset;
            public Texture _Cloud2Tex;
            public Vector2 _Cloud2TexScale;
            public Vector2 _Cloud2TexOffset;
            public Vector4 _CutoffScroll;
            public bool _VertexColors;
            public bool _LuminanceForVertexAlpha;
            public bool _LuminanceForTextureAlpha;
            public bool _VertexOffset;
            public bool _FresnelFade;
            public bool _SkyboxOnly;

            [Range(-20f, 20f)]
            public float _FresnelPower;

            [Range(0f, 3f)]
            public float _VertexOffsetAmount;

            public void Start()
            {
                GrabMaterialValues();
            }

            public void GrabMaterialValues()
            {
                if (material)
                {
                    _SrcBlend = (_BlendEnums)(int)material.GetFloat("_SrcBlend");
                    _DstBlend = (_BlendEnums)(int)material.GetFloat("_DstBlend");
                    _Tint = material.GetColor("_TintColor");
                    _DisableRemapping = material.IsKeywordEnabled("DISABLEREMAP");
                    _MainTex = material.GetTexture("_MainTex");
                    _MainTexScale = material.GetTextureScale("_MainTex");
                    _MainTexOffset = material.GetTextureOffset("_MainTex");
                    _RemapTex = material.GetTexture("_RemapTex");
                    _RemapTexScale = material.GetTextureScale("_RemapTex");
                    _RemapTexOffset = material.GetTextureOffset("_RemapTex");
                    _SoftFactor = material.GetFloat("_InvFade");
                    _BrightnessBoost = material.GetFloat("_Boost");
                    _AlphaBoost = material.GetFloat("_AlphaBoost");
                    _AlphaBias = material.GetFloat("_AlphaBias");
                    _UseUV1 = material.IsKeywordEnabled("USE_UV1");
                    _FadeWhenNearCamera = material.IsKeywordEnabled("FADECLOSE");
                    _FadeCloseDistance = material.GetFloat("_FadeCloseDistance");
                    _Cull_Mode = (_CullEnum)(int)material.GetFloat("_Cull");
                    _ZTest_Mode = (_ZTestEnum)(int)material.GetFloat("_ZTest");
                    _DepthOffset = material.GetFloat("_DepthOffset");
                    _CloudRemapping = material.IsKeywordEnabled("USE_CLOUDS");
                    _DistortionClouds = material.IsKeywordEnabled("CLOUDOFFSET");
                    _DistortionStrength = material.GetFloat("_DistortionStrength");
                    _Cloud1Tex = material.GetTexture("_Cloud1Tex");
                    _Cloud1TexScale = material.GetTextureScale("_Cloud1Tex");
                    _Cloud1TexOffset = material.GetTextureOffset("_Cloud1Tex");
                    _Cloud2Tex = material.GetTexture("_Cloud2Tex");
                    _Cloud2TexScale = material.GetTextureScale("_Cloud2Tex");
                    _Cloud2TexOffset = material.GetTextureOffset("_Cloud2Tex");
                    _CutoffScroll = material.GetVector("_CutoffScroll");
                    _VertexColors = material.IsKeywordEnabled("VERTEXCOLOR");
                    _LuminanceForVertexAlpha = material.IsKeywordEnabled("VERTEXALPHA");
                    _LuminanceForTextureAlpha = material.IsKeywordEnabled("CALCTEXTUREALPHA");
                    _VertexOffset = material.IsKeywordEnabled("VERTEXOFFSET");
                    _FresnelFade = material.IsKeywordEnabled("FRESNEL");
                    _SkyboxOnly = material.IsKeywordEnabled("SKYBOX_ONLY");
                    _FresnelPower = material.GetFloat("_FresnelPower");
                    _VertexOffsetAmount = material.GetFloat("_OffsetAmount");
                    MaterialName = material.name;
                }
            }



            public void Update()
            {

                if (material)
                {
                    if (material.name != MaterialName && renderer)
                    {
                        GrabMaterialValues();
                        PutMaterialIntoMeshRenderer(renderer, material);
                    }

                    material.SetFloat("_SrcBlend", Convert.ToSingle(_SrcBlend));
                    material.SetFloat("_DstBlend", Convert.ToSingle(_DstBlend));

                    material.SetColor("_TintColor", _Tint);

                    SetShaderKeywordBasedOnBool(_DisableRemapping, material, "DISABLEREMAP");

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

                    material.SetFloat("_InvFade", _SoftFactor);
                    material.SetFloat("_Boost", _BrightnessBoost);
                    material.SetFloat("_AlphaBoost", _AlphaBoost);
                    material.SetFloat("_AlphaBias", _AlphaBias);

                    SetShaderKeywordBasedOnBool(_UseUV1, material, "USE_UV1");
                    SetShaderKeywordBasedOnBool(_FadeWhenNearCamera, material, "FADECLOSE");

                    material.SetFloat("_FadeCloseDistance", _FadeCloseDistance);
                    material.SetFloat("_Cull", Convert.ToSingle(_Cull_Mode));
                    material.SetFloat("_ZTest", Convert.ToSingle(_ZTest_Mode));
                    material.SetFloat("_DepthOffset", _DepthOffset);

                    SetShaderKeywordBasedOnBool(_CloudRemapping, material, "USE_CLOUDS");
                    SetShaderKeywordBasedOnBool(_DistortionClouds, material, "CLOUDOFFSET");

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

                    SetShaderKeywordBasedOnBool(_VertexColors, material, "VERTEXCOLOR");
                    SetShaderKeywordBasedOnBool(_LuminanceForVertexAlpha, material, "VERTEXALPHA");
                    SetShaderKeywordBasedOnBool(_LuminanceForTextureAlpha, material, "CALCTEXTUREALPHA");
                    SetShaderKeywordBasedOnBool(_VertexOffset, material, "VERTEXOFFSET");
                    SetShaderKeywordBasedOnBool(_FresnelFade, material, "FRESNEL");
                    SetShaderKeywordBasedOnBool(_SkyboxOnly, material, "SKYBOX_ONLY");

                    material.SetFloat("_FresnelPower", _FresnelPower);
                    material.SetFloat("_OffsetAmount", _VertexOffsetAmount);
                }
            }
        }

        public class HGIntersectionController : MaterialController
        {
            public enum _SrcBlendFloatEnum
            {
                Zero = 0,
                One = 1,
                DstColor = 2,
                SrcColor = 3,
                OneMinusDstColor = 4,
                SrcAlpha = 5,
                OneMinusSrcColor = 6,
                DstAlpha = 7,
                OneMinusDstAlpha = 8,
                SrcAlphaSaturate = 9,
                OneMinusSrcAlpha = 10
            }
            public enum _DstBlendFloatEnum
            {
                Zero = 0,
                One = 1,
                DstColor = 2,
                SrcColor = 3,
                OneMinusDstColor = 4,
                SrcAlpha = 5,
                OneMinusSrcColor = 6,
                DstAlpha = 7,
                OneMinusDstAlpha = 8,
                SrcAlphaSaturate = 9,
                OneMinusSrcAlpha = 10
            }
            public _SrcBlendFloatEnum _Source_Blend_Mode;
            public _DstBlendFloatEnum _Destination_Blend_Mode;

            public Color _Tint;
            public Texture _MainTex;
            public Vector2 _MainTexScale;
            public Vector2 _MainTexOffset;
            public Texture _Cloud1Tex;
            public Vector2 _Cloud1TexScale;
            public Vector2 _Cloud1TexOffset;
            public Texture _Cloud2Tex;
            public Vector2 _Cloud2TexScale;
            public Vector2 _Cloud2TexOffset;
            public Texture _RemapTex;
            public Vector2 _RemapTexScale;
            public Vector2 _RemapTexOffset;
            public Vector4 _CutoffScroll;

            [Range(0f, 30f)]
            public float _SoftFactor;

            [Range(0.1f, 20f)]
            public float _SoftPower;

            [Range(0f, 5f)]
            public float _BrightnessBoost;

            [Range(0.1f, 20f)]
            public float _RimPower;

            [Range(0f, 5f)]
            public float _RimStrength;

            [Range(0f, 20f)]
            public float _AlphaBoost;

            [Range(0f, 20f)]
            public float _IntersectionStrength;

            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull_Mode;

            public bool _FadeFromVertexColorsOn;
            public bool _EnableTriplanarProjectionsForClouds;

            public void Start()
            {
                GrabMaterialValues();
            }

            public void GrabMaterialValues()
            {
                if (material)
                {
                    _Source_Blend_Mode = (_SrcBlendFloatEnum)(int)material.GetFloat("_SrcBlendFloat");
                    _Destination_Blend_Mode = (_DstBlendFloatEnum)(int)material.GetFloat("_DstBlendFloat");
                    _Tint = material.GetColor("_TintColor");
                    _MainTex = material.GetTexture("_MainTex");
                    _MainTexScale = material.GetTextureScale("_MainTex");
                    _MainTexOffset = material.GetTextureOffset("_MainTex");
                    _Cloud1Tex = material.GetTexture("_Cloud1Tex");
                    _Cloud1TexScale = material.GetTextureScale("_Cloud1Tex");
                    _Cloud1TexOffset = material.GetTextureOffset("_Cloud1Tex");
                    _Cloud2Tex = material.GetTexture("_Cloud2Tex");
                    _Cloud2TexScale = material.GetTextureScale("_Cloud2Tex");
                    _Cloud2TexOffset = material.GetTextureOffset("_Cloud2Tex");
                    _RemapTex = material.GetTexture("_RemapTex");
                    _RemapTexScale = material.GetTextureScale("_RemapTex");
                    _RemapTexOffset = material.GetTextureOffset("_RemapTex");
                    _CutoffScroll = material.GetVector("_CutoffScroll");
                    _SoftFactor = material.GetFloat("_InvFade");
                    _SoftPower = material.GetFloat("_SoftPower");
                    _BrightnessBoost = material.GetFloat("_Boost");
                    _RimPower = material.GetFloat("_RimPower");
                    _RimStrength = material.GetFloat("_RimStrength");
                    _AlphaBoost = material.GetFloat("_AlphaBoost");
                    _IntersectionStrength = material.GetFloat("_IntersectionStrength");
                    _Cull_Mode = (_CullEnum)(int)material.GetFloat("_Cull");
                    _FadeFromVertexColorsOn = material.IsKeywordEnabled("FADE_FROM_VERTEX_COLORS");
                    _EnableTriplanarProjectionsForClouds = material.IsKeywordEnabled("TRIPLANAR");
                    MaterialName = material.name;
                }
            }

            public void Update()
            {

                if (material)
                {
                    if (material.name != MaterialName && renderer)
                    {
                        GrabMaterialValues();
                        PutMaterialIntoMeshRenderer(renderer, material);
                    }
                    material.SetFloat("_SrcBlendFloat", Convert.ToSingle(_Source_Blend_Mode));
                    material.SetFloat("_DstBlendFloat", Convert.ToSingle(_Destination_Blend_Mode));
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

                    material.SetVector("_CutoffScroll", _CutoffScroll);
                    material.SetFloat("_InvFade", _SoftFactor);
                    material.SetFloat("_SoftPower", _SoftPower);
                    material.SetFloat("_Boost", _BrightnessBoost);
                    material.SetFloat("_RimPower", _RimPower);
                    material.SetFloat("_RimStrength", _RimStrength);
                    material.SetFloat("_AlphaBoost", _AlphaBoost);
                    material.SetFloat("_IntersectionStrength", _IntersectionStrength);
                    material.SetFloat("_Cull", Convert.ToSingle(_Cull_Mode));

                    SetShaderKeywordBasedOnBool(_FadeFromVertexColorsOn, material, "FADE_FROM_VERTEX_COLORS");
                    SetShaderKeywordBasedOnBool(_EnableTriplanarProjectionsForClouds, material, "TRIPLANAR");
                }
            }
        }

        public class HGSolidParallaxController : MaterialController
        {
            public Vector4 _Color;

            public Texture _MainTex;
            public Vector2 _MainTexScale;
            public Vector2 _MainTexOffset;

            public Texture _EmissionTex;
            public Vector2 _EmissionTexScale;
            public Vector2 _EmissionTexOffset;
            [Range(0.1f, 20f)]
            public float _EmissionPower;

            public Texture _Normal;
            public Vector2 _NormalScale;
            public Vector2 _NormalOffset;

            [Range(0f, 1f)]
            public float _SpecularStrength;
            [Range(0.1f, 20f)]
            public float _SpecularExponent;

            [Range(0f, 1f)]
            public float _Smoothness;

            public Texture _Height1;
            public Vector2 _Height1Scale;
            public Vector2 _Height1Offset;
            public Texture _Height2;
            public Vector2 _Height2Scale;
            public Vector2 _Height2Offset;

            [Range(0, 20f)]
            public float _HeightStrength;
            [Range(0f, 1f)]
            public float _HeightBias;

            public Vector4 _ScrollSpeed;

            public float _Parallax;
            public enum _RampEnum
            {
                TwoTone = 0,
                SmoothedTwoTone = 1,
                Unlitish = 3,
                Subsurface = 4,
                Grass = 5
            }
            public _RampEnum _RampInfo;
            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull_Mode;

            public bool _AlphaClip;



            public void Start()
            {
                GrabMaterialValues();
            }

            public void GrabMaterialValues()
            {
                if (material)
                {
                    _Color = material.GetColor("_Color");
                    _MainTex = material.GetTexture("_MainTex");
                    _MainTexScale = material.GetTextureScale("_MainTex");
                    _MainTexOffset = material.GetTextureOffset("_MainTex");
                    _EmissionTex = material.GetTexture("_EmissionTex");
                    _EmissionTexScale = material.GetTextureScale("_EmissionTex");
                    _EmissionTexOffset = material.GetTextureOffset("_EmissionTex");
                    _EmissionPower = material.GetFloat("_EmissionPower");
                    _Normal = material.GetTexture("_Normal");
                    _NormalScale = material.GetTextureScale("_Normal");
                    _NormalOffset = material.GetTextureOffset("_Normal");
                    _Smoothness = material.GetFloat("_Smoothness");
                    _SpecularStrength = material.GetFloat("_SpecularStrength");
                    _SpecularExponent = material.GetFloat("_SpecularExponent");
                    _Cull_Mode = (_CullEnum)(int)material.GetFloat("_Cull");
                    _RampInfo = (_RampEnum)(int)material.GetFloat("_RampInfo");
                    _Height1 = material.GetTexture("_Height1");
                    _Height1Scale = material.GetTextureScale("_Height1");
                    _Height1Offset = material.GetTextureOffset("_Height1");
                    _Height2 = material.GetTexture("_Height2");
                    _Height2Scale = material.GetTextureScale("_Height2");
                    _Height2Offset = material.GetTextureOffset("_Height2");
                    _HeightStrength = material.GetFloat("_HeightStrength");
                    _HeightBias = material.GetFloat("_HeightBias");
                    _Parallax = material.GetFloat("_Parallax");
                    _ScrollSpeed = material.GetVector("_ScrollSpeed");
                    _AlphaClip = material.IsKeywordEnabled("ALPHACLIP");
                    MaterialName = material.name;
                }
            }



            public void Update()
            {

                if (material)
                {
                    if (material.name != MaterialName && renderer)
                    {
                        GrabMaterialValues();
                        PutMaterialIntoMeshRenderer(renderer, material);
                    }
                    material.SetColor("_Color", _Color);

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

                    if (_EmissionTex)
                    {
                        material.SetTexture("_EmissionTex", _EmissionTex);
                        material.SetTextureScale("_EmissionTex", _EmissionTexScale);
                        material.SetTextureOffset("_EmissionTex", _EmissionTexOffset);
                    }
                    else
                    {
                        material.SetTexture("_EmissionTex", null);
                    }

                    material.SetFloat("_Smoothness", _Smoothness);
                    material.SetFloat("_EmissionPower", _EmissionPower);
                    material.SetFloat("_SpecularExponent", _SpecularExponent);
                    material.SetFloat("_SpecularStrength", _SpecularStrength);

                    material.SetFloat("_Cull", Convert.ToSingle(_Cull_Mode));
                    material.SetFloat("_RampInfo", Convert.ToSingle(_RampInfo));

                    material.SetFloat("_HeightBias", _HeightBias);

                    if (_Height1)
                    {
                        material.SetTexture("_Height1", _Height1);
                        material.SetTextureScale("_Height1", _Height1Scale);
                        material.SetTextureOffset("_Height1", _Height1Offset);
                    }
                    else
                    {
                        material.SetTexture("_Height1", null);
                    }

                    if (_Height2)
                    {
                        material.SetTexture("_Height2", _Height2);
                        material.SetTextureScale("_Height2", _Height2Scale);
                        material.SetTextureOffset("_Height2", _Height2Offset);
                    }
                    else
                    {
                        material.SetTexture("_Height2", null);
                    }

                    material.SetVector("_ScrollSpeed", _ScrollSpeed);

                    SetShaderKeywordBasedOnBool(_AlphaClip, material, "ALPHACLIP");

                    material.SetFloat("_HeightStrength", _HeightStrength);
                    material.SetFloat("_Parallax", _Parallax);
                }
            }
        }

        public class HGWavyClothController : MaterialController
        {
            public Vector4 _Color;
            [Range(0f, 1f)]
            public float _Cutoff;
            public Texture _MainTex;
            public Vector2 _MainTexScale;
            public Vector2 _MainTexOffset;
            public Texture _ScrollingNormalMap;
            public Vector2 _NormalScale;
            public Vector2 _NormalOffset;
            [Range(0f, 5f)]
            public float _NormalStrength;
            public Vector4 _Scroll;
            [Range(0f, 5f)]
            public float _VertexOffsetStrength;
            public Vector4 _WindVector;
            [Range(0f, 1f)]
            public float _Smoothness;
            public enum _RampEnum
            {
                TwoTone = 0,
                SmoothedTwoTone = 1,
                Unlitish = 3,
                Subsurface = 4,
                Grass = 5
            }
            public _RampEnum _RampInfo;
            [Range(0f, 1f)]
            public float _SpecularStrength;
            [Range(0.1f, 20f)]
            public float _SpecularExponent;
            public bool _EnableVertexColorDistortion;

            public void Start()
            {
                GrabMaterialValues();
            }

            public void GrabMaterialValues()
            {
                if (material)
                {
                    _Color = material.GetColor("_Color");
                    _Cutoff = material.GetFloat("_Cutoff");
                    _MainTex = material.GetTexture("_MainTex");
                    _MainTexScale = material.GetTextureScale("_MainTex");
                    _MainTexOffset = material.GetTextureOffset("_MainTex");
                    _ScrollingNormalMap = material.GetTexture("_ScrollingNormalMap");
                    _NormalScale = material.GetTextureScale("_ScrollingNormalMap");
                    _NormalOffset = material.GetTextureOffset("_ScrollingNormalMap");
                    _NormalStrength = material.GetFloat("_NormalStrength");
                    _Scroll = material.GetVector("_Scroll");
                    _VertexOffsetStrength = material.GetFloat("_VertexOffsetStrength");
                    _WindVector = material.GetVector("_WindVector");
                    _Smoothness = material.GetFloat("_Smoothness");
                    _RampInfo = (_RampEnum)(int)material.GetFloat("_RampInfo");
                    _SpecularStrength = material.GetFloat("_SpecularStrength");
                    _SpecularExponent = material.GetFloat("_SpecularExponent");
                    _EnableVertexColorDistortion = material.IsKeywordEnabled("VERTEX_RED_FOR_DISTORTION");
                    MaterialName = material.name;
                }
            }



            public void Update()
            {

                if (material)
                {
                    if (material.name != MaterialName && renderer)
                    {
                        GrabMaterialValues();
                        PutMaterialIntoMeshRenderer(renderer, material);
                    }
                    material.SetColor("_Color", _Color);

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

                    if (_ScrollingNormalMap)
                    {
                        material.SetTexture("_ScrollingNormalMap", _ScrollingNormalMap);
                        material.SetTextureScale("_ScrollingNormalMap", _NormalScale);
                        material.SetTextureOffset("_ScrollingNormalMap", _NormalOffset);
                    }
                    else
                    {
                        material.SetTexture("_ScrollingNormalMap", null);
                    }



                    material.SetFloat("_Smoothness", _Smoothness);
                    material.SetFloat("_Cutoff", _Cutoff);
                    material.SetFloat("_SpecularExponent", _SpecularExponent);
                    material.SetFloat("_SpecularStrength", _SpecularStrength);
                    material.SetFloat("_VertexOffsetStrength", _VertexOffsetStrength);

                    material.SetFloat("_RampInfo", Convert.ToSingle(_RampInfo));


                    material.SetVector("_Scroll", _Scroll);
                    material.SetVector("_WindVector", _WindVector);

                    SetShaderKeywordBasedOnBool(_EnableVertexColorDistortion, material, "VERTEX_RED_FOR_DISTORTION");

                    material.SetFloat("_NormalStrength", _NormalStrength);
                }
            }
        }

        public class HGOpaqueCloudRemap : MaterialController
        {
            public Color _TintColor;
            public Color _EmissionColor;
            public Texture _MainTex;
            public Vector2 _MainTexScale;
            public Vector2 _MainTexOffset;
            [Range(0f, 5f)]
            public float _NormalStrength;
            public Texture _NormalTex;
            public Texture _Cloud1Tex;
            public Vector2 _Cloud1TexScale;
            public Vector2 _Cloud1TexOffset;
            public Texture _Cloud2Tex;
            public Vector2 _Cloud2TexScale;
            public Vector2 _Cloud2TexOffset;
            public Texture _RemapTex;
            public Vector2 _RemapTexScale;
            public Vector2 _RemapTexOffset;
            public Vector4 _CutoffScroll;
            [Range(0f, 30f)]
            public float _InvFade;
            [Range(0f, 20f)]
            public float _AlphaBoost;
            [Range(0f, 1f)]
            public float _Cutoff;
            [Range(0f, 1f)]
            public float _SpecularStrength;
            [Range(0.1f, 20f)]
            public float _SpecularExponent;
            [Range(0f, 10f)]
            public float _ExtrusionStrength;
            public enum _RampEnum
            {
                TwoTone = 0,
                SmoothedTwoTone = 1,
                Unlitish = 3,
                Subsurface = 4,
                Grass = 5
            }
            public _RampEnum _RampInfo;
            public bool _EmissionFromAlbedo;
            public bool _CloudNormalMap;
            public bool _VertexAlphaOn;
            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull;
            public float _ExternalAlpha;

            public void Start()
            {
                GrabMaterialValues();
            }
            private void GrabMaterialValues()
            {
                if (material)
                {
                    _TintColor = material.GetColor("_TintColor");
                    _EmissionColor = material.GetColor("_EmissionColor");
                    _MainTex = material.GetTexture("_MainTex");
                    _MainTexScale = material.GetTextureScale("_MainTex");
                    _MainTexOffset = material.GetTextureOffset("_MainTex");
                    _NormalStrength = material.GetFloat("_NormalStrength");
                    _NormalTex = material.GetTexture("_NormalTex");
                    _Cloud1Tex = material.GetTexture("_Cloud1Tex");
                    _Cloud1TexScale = material.GetTextureScale("_Cloud1Tex");
                    _Cloud1TexOffset = material.GetTextureOffset("_Cloud1Tex");
                    _Cloud2Tex = material.GetTexture("_Cloud2Tex");
                    _Cloud2TexScale = material.GetTextureScale("_Cloud2Tex");
                    _Cloud2TexOffset = material.GetTextureScale("_Cloud2Tex");
                    _RemapTex = material.GetTexture("_RemapTex");
                    _RemapTexScale = material.GetTextureScale("_RemapTex");
                    _RemapTexOffset = material.GetTextureOffset("_RemapTex");
                    _CutoffScroll = material.GetVector("_CutoffScroll");
                    _InvFade = material.GetFloat("_InvFade");
                    _AlphaBoost = material.GetFloat("_AlphaBoost");
                    _Cutoff = material.GetFloat("_Cutoff");
                    _SpecularStrength = material.GetFloat("_SpecularStrength");
                    _SpecularExponent = material.GetFloat("_SpecularExponent");
                    _ExtrusionStrength = material.GetFloat("_ExtrusionFloat");
                    _RampInfo = (_RampEnum)material.GetInt("_RampInfo");
                    _EmissionFromAlbedo = material.IsKeywordEnabled("EMISSIONFROMALBEDO");
                    _CloudNormalMap = material.IsKeywordEnabled("CLOUDNORMAL");
                    _VertexAlphaOn = material.IsKeywordEnabled("VERTEXALPHA");
                    _Cull = (_CullEnum)material.GetInt("_Cull");
                    _ExternalAlpha = material.GetFloat("_ExternalAlpha");
                }
            }

            public void Update()
            {
                if (material)
                {
                    if (material.name != MaterialName && renderer)
                    {
                        GrabMaterialValues();
                        PutMaterialIntoMeshRenderer(renderer, material);
                    }
                }
                material.SetColor("_TintColor", _TintColor);
                material.SetColor("_EmissionColor", _EmissionColor);

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

                material.SetFloat("_NormalStrength", _NormalStrength);

                if (_NormalTex)
                {
                    material.SetTexture("_NormalTex", _NormalTex);
                }
                else
                {
                    material.SetTexture("_NormalTex", null);
                }

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

                material.SetVector("_CutoffScroll", _CutoffScroll);
                material.SetFloat("_InvFade", _InvFade);
                material.SetFloat("_AlphaBoost", _AlphaBoost);
                material.SetFloat("_Cutoff", _Cutoff);
                material.SetFloat("_SpecularStrength", _SpecularStrength);
                material.SetFloat("_SpecularExponent", _SpecularExponent);
                material.SetFloat("_ExtrusionStrength", _ExtrusionStrength);
                material.SetInt("_RampInfo", (int)_RampInfo);
                SetShaderKeywordBasedOnBool(_EmissionFromAlbedo, material, "EMISSIONFROMALBEDO");
                SetShaderKeywordBasedOnBool(_CloudNormalMap, material, "CLOUDNORMAL");
                SetShaderKeywordBasedOnBool(_VertexAlphaOn, material, "VERTEXALPHA");
                material.SetInt("_Cull", (int)_Cull);
                material.SetFloat("_ExternalAlpha", _ExternalAlpha);
            }
        }
        
        public class HGTriplanarController : MaterialController
        {
            public bool _ColorsOn;
            public bool _MixColorsOn;
            public bool _MaskOn;
            public bool _VerticalBiasOn;
            public bool _DoubleSampleOn;

            public Color _Color;
            public Texture _NormalTex;

            [Range(0f, 1f)]
            public float _NormalStrength;

            public enum _RampInfoEnum
            {
                TwoTone = 0,
                SmoothedTwoTone = 1,
                Unlitish = 2,
                Subsurface = 3,
                Grass = 4
            }
            public _RampInfoEnum _RampInfo;

            public enum _DecalLayerEnum
            {
                Default = 0,
                Environment = 1,
                Character = 2,
                Misc = 3
            }
            public _DecalLayerEnum _DecalLayer;

            public enum _CullEnum
            {
                Off = 0,
                Front = 1,
                Back = 2
            }
            public _CullEnum _Cull;

            [Range(0f, 1f)]
            public float _TextureFactor;

            [Range(0f, 1f)]
            public float _Depth;

            public Texture _SplatmapTex;

            public Texture _RedChannelTopTex;
            public Texture _RedChannelSideTex;
            [Range(0f, 1f)]
            public float _RedChannelSmoothness;
            [Range(0f, 1f)]
            public float _RedChannelSpecularStrength;
            [Range(0.1f, 20f)]
            public float _RedChannelSpecularExponent;
            [Range(-2f, 5f)]
            public float _RedChannelBias;

            public Texture _GreenChannelTex;
            [Range(0f, 1f)]
            public float _GreenChannelSmoothness;
            [Range(0f, 1f)]
            public float _GreenChannelSpecularStrength;
            [Range(0.1f, 20f)]
            public float _GreenChannelSpecularExponent;
            [Range(-2f, 5f)]
            public float _GreenChannelBias;

            public Texture _BlueChannelTex;
            [Range(0f, 1f)]
            public float _BlueChannelSmoothness;
            [Range(0f, 1f)]
            public float _BlueChannelSpecularStrength;
            [Range(0.1f, 20f)]
            public float _BlueChannelSpecularExponent;
            [Range(-2f, 5f)]
            public float _BlueChannelBias;

            public bool _SnowOn;

            public void Start()
            {
                GrabMaterialValues();
            }

            public void GrabMaterialValues()
            {
                if (material)
                {
                    _ColorsOn = material.IsKeywordEnabled("USE_VERTEX_COLORS");
                    _MixColorsOn = material.IsKeywordEnabled("MIX_VERTEX_COLORS");
                    _MaskOn = material.IsKeywordEnabled("USE_ALPHA_AS_MASK");
                    _VerticalBiasOn = material.IsKeywordEnabled("USE_VERTICAL_BIAS");
                    _DoubleSampleOn = material.IsKeywordEnabled("DOUBLESAMPLE");
                    _Color = material.GetColor("_Color");//
                    _NormalTex = material.GetTexture("_NormalTex");//
                    _NormalStrength = material.GetFloat("_NormalStrength");//
                    _RampInfo = (_RampInfoEnum)(int)material.GetFloat("_RampInfo");//
                    _DecalLayer = (_DecalLayerEnum)(int)material.GetFloat("_DecalLayer");//
                    _Cull = (_CullEnum)(int)material.GetFloat("_Cull");//
                    _TextureFactor = material.GetFloat("_TextureFactor");//
                    _Depth = material.GetFloat("_Depth");
                    _SplatmapTex = material.GetTexture("_SplatmapTex");
                    _RedChannelTopTex = material.GetTexture("_RedChannelTopTex");
                    _RedChannelSideTex = material.GetTexture("_RedChannelSideTex");
                    _RedChannelSmoothness = material.GetFloat("_RedChannelSmoothness");
                    _RedChannelSpecularStrength = material.GetFloat("_RedChannelSpecularStrength");//
                    _RedChannelSpecularExponent = material.GetFloat("_RedChannelSpecularExponent");//
                    _RedChannelBias = material.GetFloat("_RedChannelBias");
                    _GreenChannelTex = material.GetTexture("_GreenChannelTex");
                    _GreenChannelSmoothness = material.GetFloat("_GreenChannelSmoothness");
                    _GreenChannelSpecularStrength = material.GetFloat("_GreenChannelSpecularStrength");//
                    _GreenChannelSpecularExponent = material.GetFloat("_GreenChannelSpecularExponent");//
                    _GreenChannelBias = material.GetFloat("_GreenChannelBias");
                    _BlueChannelTex = material.GetTexture("_BlueChannelTex");
                    _BlueChannelSmoothness = material.GetFloat("_BlueChannelSmoothness");
                    _BlueChannelSpecularStrength = material.GetFloat("_BlueChannelSpecularStrength");//
                    _BlueChannelSpecularExponent = material.GetFloat("_BlueChannelSpecularExponent");//
                    _BlueChannelBias = material.GetFloat("_BlueChannelBias");
                    _SnowOn = material.IsKeywordEnabled("MICROFACET_SNOW");
                }
            }

            public void Update()
            {
            
                if (material)
                {
                    if(material.name != MaterialName && renderer)
                    {
                        GrabMaterialValues();
                        MaterialControllerComponents.PutMaterialIntoMeshRenderer(renderer, material);
                    }

                    material.SetColor("_Color", _Color);

                    material.SetFloat("_NormalStrength", _NormalStrength);

                    if (_NormalTex)
                    {
                        material.SetTexture("_NormalText", _NormalTex);
                    }
                    else
                    {
                        material.SetTexture("_NormalTex", null);
                    }

                    material.SetFloat("_RampInfo", Convert.ToSingle(_RampInfo));
                    material.SetFloat("_DecalLayer", Convert.ToSingle(_DecalLayer));

                    if (_RedChannelTopTex)
                        material.SetTexture("_RedChannelTopTex", _RedChannelTopTex);
                    else
                        material.SetTexture("_RedChannelTopTex", null);
                    if (_RedChannelSideTex)
                        material.SetTexture("_RedChannelSideTex", _RedChannelSideTex);
                    else
                        material.SetTexture("_RedChannelSideTex", null);
                    material.SetFloat("_RedChannelSmoothness", _RedChannelSmoothness);
                    material.SetFloat("_RedChannelSpecularStrength", _RedChannelSpecularStrength);
                    material.SetFloat("_RedChannelSpecularExponent", _RedChannelSpecularExponent);
                    material.SetFloat("_RedChannelBias", _RedChannelBias);

                    if (_GreenChannelTex)
                    material.SetTexture("_GreenChannelTex", _GreenChannelTex);
                    else
                        material.SetTexture("_GreenChannelTex", null);
                    material.SetFloat("_GreenChannelSmoothness", _GreenChannelSmoothness);
                    material.SetFloat("_GreenChannelSpecularStrength", _GreenChannelSpecularStrength);
                    material.SetFloat("_GreenChannelSpecularExponent", _GreenChannelSpecularExponent);
                    material.SetFloat("_GreenChannelBias", _GreenChannelBias);

                    if (_BlueChannelTex)
                        material.SetTexture("_BlueChannelTex", _BlueChannelTex);
                    else
                        material.SetTexture("_BlueChannelTex", null);
                    material.SetFloat("_BlueChannelSmoothness", _BlueChannelSmoothness);
                    material.SetFloat("_BlueChannelSpecularStrength", _BlueChannelSpecularStrength);
                    material.SetFloat("_BlueChannelSpecularExponent", _BlueChannelSpecularExponent);
                    material.SetFloat("_BlueChannelBias", _BlueChannelBias);


                    material.SetFloat("_Cull", Convert.ToSingle(_Cull));
                    material.SetFloat("_TextureFactor", _TextureFactor);
                    material.SetFloat("_Depth", _Depth);

                    if (_SplatmapTex)
                    material.SetTexture("_SplatmapTex", _SplatmapTex);
                    else
                        material.SetTexture("_SplatmapTex", null);

                    MaterialControllerComponents.SetShaderKeywordBasedOnBool(_SnowOn, material, "MICROFACET_SNOW");
                    MaterialControllerComponents.SetShaderKeywordBasedOnBool(_ColorsOn, material, "USE_VERTEX_COLORS");
                    MaterialControllerComponents.SetShaderKeywordBasedOnBool(_MixColorsOn, material, "MIX_VERTEX_COLORS");
                    MaterialControllerComponents.SetShaderKeywordBasedOnBool(_MaskOn, material, "USE_ALPHA_AS_MASK");
                    MaterialControllerComponents.SetShaderKeywordBasedOnBool(_VerticalBiasOn, material, "USE_VERTICAL_BIAS");
                    MaterialControllerComponents.SetShaderKeywordBasedOnBool(_DoubleSampleOn, material, "DOUBLESAMPLE");
                }
            }
        }
        
        public class CW_DX11_DoubleSidedController : MaterialController
        {
            public Color _Color;
            public Color _DepthColor;
            public float _Depth;
            public bool _EnableFog;
            public float _EdgeFade;
            public Color _SpecColor;
            [Range(0.01f, 5f)]
            public float _Smoothness;

            public Texture _BumpMap;
            [Range(0f, 1f)]
            public float _BumpStrength;
            public bool _EnableLargeBump;
            public Texture _BumpMapLarge;
            [Range(0f, 1f)]
            public float _BumpLargeStrength;
            public bool _WorldSpace;

            public Vector4 _Speeds;
            public Vector4 _SpeedsLarge;
            public float _Distortion;

            public enum _DistortionQualityEnum
            {
                High = 0,
                Low = 1
            }
            public _DistortionQualityEnum _DistortionQuality;

            public enum _ReflectionTypeEnum
            {
                None = 0,
                Mixed = 1,
                RealTime = 2,
                CubeMap = 3
            }
            public _ReflectionTypeEnum _ReflectionType;

            public Color _CubeColor;
            public Texture _Cube;
            public Texture _ReflectionTex;
            [Range(0f, 1f)]
            public float _Reflection;
            [Range(1f, 20f)]
            public float _RimPower;

            public bool _FOAM;
            public Color _FoamColor;
            public Texture _FoamTex;
            public float _FoamSize;

            public enum _DisplacementModeEnum
            {
                Off = 0,
                Wave = 1,
                Gerstner = 2
            }
            public _DisplacementModeEnum _DisplacementMode;

            public float _Amplitude;
            public float _Frequency;
            public float _Speed;
            public float _Steepness;
            public Vector4 _WSpeed;
            public Vector4 _WDirectionAB;
            public Vector4 _WDirectionCD;

            [Range(0f, 1f)]
            public float _Smoothing;

            [Range(1f, 32f)]
            public float _Tess;

            public void Start()
            {
                GrabMaterialValues();
            }

            public void GrabMaterialValues()
            {
                if (material)
                {
                    _Color = material.GetColor("_Color");
                    _DepthColor = material.GetColor("_DepthColor");
                    _Depth = material.GetFloat("_Depth");
                    _EnableFog = material.IsKeywordEnabled("_DEPTHFOG_ON");
                    _EdgeFade = material.GetFloat("_EdgeFade");
                    _SpecColor = material.GetColor("_SpecColor");
                    _Smoothness = material.GetFloat("_Smoothness");
                    _BumpMap = material.GetTexture("_BumpMap");
                    _BumpStrength = material.GetFloat("_BumpStrength");
                    _EnableLargeBump = material.IsKeywordEnabled("_BUMPLARGE_ON");
                    _BumpMapLarge = material.GetTexture("_BumpMapLarge");
                    _BumpLargeStrength = material.GetFloat("_BumpLargeStrength");
                    _WorldSpace = material.IsKeywordEnabled("_WORLDSPACE_ON");
                    _Speeds = material.GetVector("_Speeds");
                    _SpeedsLarge = material.GetVector("_SpeedsLarge");
                    _Distortion = material.GetFloat("_Distortion");
                    _DistortionQuality = (_DistortionQualityEnum)(int)material.GetFloat("_DistortionQuality");
                    _ReflectionType = (_ReflectionTypeEnum)(int)material.GetFloat("_ReflectionType");
                    _CubeColor = material.GetColor("_CubeColor");
                    _Cube = material.GetTexture("_Cube");
                    _ReflectionTex = material.GetTexture("_ReflectionTex");
                    _Reflection = material.GetFloat("_Reflection");
                    _RimPower = material.GetFloat("_RimPower");
                    _FOAM = material.IsKeywordEnabled("_FOAM_ON");
                    _FoamColor = material.GetColor("_FoamColor");
                    _FoamTex = material.GetTexture("_FoamTex");
                    _FoamSize = material.GetFloat("_FoamSize");
                    _DisplacementMode = (_DisplacementModeEnum)(int)material.GetFloat("_DisplacementMode");
                    _Amplitude = material.GetFloat("_Amplitude");
                    _Frequency = material.GetFloat("_Frequency");
                    _Speed = material.GetFloat("_Speed");
                    _Steepness = material.GetFloat("_Steepness");
                    _WSpeed = material.GetVector("_WSpeed");
                    _WDirectionAB = material.GetVector("_WDirectionAB");
                    _WDirectionCD = material.GetVector("_WDirectionCD");
                    _Smoothing = material.GetFloat("_Smoothing");
                    _Tess = material.GetFloat("_Tess");
                }
            }

            public void Update()
            {

                if (material)
                {
                    if (material.name != MaterialName && renderer)
                    {
                        GrabMaterialValues();
                        MaterialControllerComponents.PutMaterialIntoMeshRenderer(renderer, material);
                    }

                    material.SetColor("_Color", _Color);
                    material.SetColor("_DepthColor", _DepthColor);
                    material.SetFloat("_Depth", _Depth);
                    MaterialControllerComponents.SetShaderKeywordBasedOnBool(_EnableFog, material, "_DEPTHFOG_ON");
                    material.SetFloat("_EdgeFade", _EdgeFade);
                    material.SetVector("_SpecColor", _SpecColor);
                    material.SetFloat("_Smoothness", _Smoothness);
                    if (_BumpMap)
                        material.SetTexture("_BumpMap", _BumpMap);
                    else
                        material.SetTexture("_BumpMap", null);
                    material.SetFloat("_BumpStrength", _BumpStrength);
                    MaterialControllerComponents.SetShaderKeywordBasedOnBool(_EnableLargeBump, material, "_BUMPLARGE_ON");
                    if (_BumpMapLarge)
                        material.SetTexture("_BumpMapLarge", _BumpMapLarge);
                    else
                        material.SetTexture("_BumpMapLarge", null);
                    material.SetFloat("_BumpLargeStrength", _BumpLargeStrength);
                    MaterialControllerComponents.SetShaderKeywordBasedOnBool(_WorldSpace, material, "_WORLDSPACE_ON");
                    material.SetVector("_Speeds", _Speeds);
                    material.SetVector("_SpeedsLarge", _SpeedsLarge);
                    material.SetFloat("_Distortion", _Distortion);
                    material.SetFloat("_DistortionQuality", Convert.ToSingle(_DistortionQuality));
                    material.SetFloat("_ReflectionType", Convert.ToSingle(_ReflectionType));
                    material.SetColor("_CubeColor", _CubeColor);
                    if (_Cube)
                        material.SetTexture("_Cube", _Cube);
                    else
                        material.SetTexture("_Cube", null);
                    if (_ReflectionTex)
                        material.SetTexture("_ReflectionTex", _ReflectionTex);
                    else
                        material.SetTexture("_ReflectionTex", _ReflectionTex);
                    material.SetFloat("_Reflection", _Reflection);
                    material.SetFloat("_RimPower", _RimPower);
                    MaterialControllerComponents.SetShaderKeywordBasedOnBool(_FOAM, material, "_FOAM_ON");
                    material.SetColor("_FoamColor", _FoamColor);
                    if (_FoamTex)
                        material.SetTexture("_FoamTex", _FoamTex);
                    else
                        material.SetTexture("_FoamTex", _FoamTex);
                    material.SetFloat("_FoamSize", _FoamSize);
                    material.SetFloat("_DisplacementMode", Convert.ToSingle(_DisplacementMode));
                    material.SetFloat("_Amplitude", _Amplitude);
                    material.SetFloat("_Frequency", _Frequency);
                    material.SetFloat("_Speed", _Speed);
                    material.SetFloat("_Steepness", _Steepness);
                    material.SetVector("_WSpeed", _WSpeed);
                    material.SetVector("_WDirectionAB", _WDirectionAB);
                    material.SetVector("_WDirectionCD", _WDirectionCD);
                    material.SetFloat("_Smoothing", _Smoothing);
                    material.SetFloat("_Tess", _Tess);
                }
            }
        }
    }
}
#endif
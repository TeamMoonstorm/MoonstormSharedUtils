#if DEBUG
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm.Components
{
    public class AllInOneMaterial : MonoBehaviour
    {
        public static readonly Dictionary<Shader, ShaderPropertyCategory[]> shaderPropertyDataSets = new Dictionary<Shader, ShaderPropertyCategory[]>
        {
            #region hgcloudremap
            {
                Resources.Load<Shader>("shaders/fx/hgcloudremap"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_TintColor", "Tint" },
                            {"_DisableRemapOn", "Disable Remapping" },
                            {"_MainTex", "Base (RGB) Trans (A)" },
                            {"_RemapTex", "Color Remap Ramp (RGB)" },
                            {"_InvFade", "Soft Factor" },
                            {"_Boost", "Brightness Boost" },
                            {"_AlphaBoost", "Alpha Boost" },
                            {"_AlphaBias", "Alpha Bias" },
                            {"_UseUV1On", "Use UV1" },
                            {"_FadeCloseOn", "Fade when near camera" },
                            {"_FadeCloseDistance", "Fade Close Distance" },
                            {"_Cull", "Culling Mode" },
                            {"_ZTest", "ZTest" },
                            {"_DepthOffset", "_DepthOffset" },
                            {"_CloudsOn", "Cloud Remapping" },
                            {"_CloudOffsetOn", "Distortion Clouds" },
                            {"_DistortionStrength", "Distortion Strength" },
                            {"_Cloud1Tex", "Cloud 1 (RGB) Trans (A)" },
                            {"_Cloud2Tex", "Cloud 2 (RGB) Trans (A)" },
                            {"_CutoffScroll", "Cutoff Scroll Speed" },
                            {"_VertexColorOn", "Vertex Colors" },
                            {"_VertexAlphaOn", "Luminance for Vertex Alpha" },
                            {"_CalcTextureAlphaOn", "Luminance for Texture Alpha" },
                            {"_VertexOffsetOn", "Vertex Offset" },
                            {"_FresnelOn", "Fresnel Fade" },
                            {"_SkyboxOnly", "Skybox Only" },
                            {"_FresnelPower", "Fresnel Power" },
                            {"_OffsetAmount", "Vertex Offset Amount" }
                        })
                }
            },
            #endregion
            #region hgdamagenumber
            {
                Resources.Load<Shader>("shaders/fx/hgdamagenumber"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_TintColor", "Tint"},
                            {"_CritColor", "Crit Color"},
                            {"_MainTex", "Texture"},
                            {"_CharacterLimit", "Character Limit"}
                        })
                }
            },
            #endregion
            #region hgdistantwater
            {
                Resources.Load<Shader>("shaders/environment/hgdistantwater"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_Color", "Main Color"},
                            {"_MainTex", "Base (RGB)"},
                            {"_NormalStrength", "Normal Strength"},
                            {"_Normal1Tex", "Normal Map 1"},
                            {"_Normal2Tex", "Normal Map 2"},
                            {"_Scroll", "Scroll Speed"},
                            {"_VertexOffsetStrength", "Vertex Offset Strength"},
                            {"_Smoothness", "Smoothness"},
                            {"_RampInfo", "Ramp Choice"},
                            {"_SpecularStrength", "Specular Strength"},
                            {"_SpecularExponent", "Specular Exponent"}
                        })
                }
            },
            #endregion
            #region hgdistortion
            {
                Resources.Load<Shader>("shaders/fx/hgdistortion"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_BumpMap", "Bump Texture"},
                            {"_MaskTex", "Mask Texture"},
                            {"_Magnitude", "Magnitude"},
                            {"_NearFadeZeroDistance", "Near-Fade Zero Distance"},
                            {"_NearFadeOneDistance", "Near-Fade One Distance"},
                            {"_FarFadeOneDistance", "Far-Fade One Distance"},
                            {"_FarFadeZeroDistance", "Far-Fade Zero Distance"},
                            {"_DistanceModulationOn", "Apply distance modulation"},
                            {"_DistanceModulationMagnitude", "Distance Modulation Magnitude"},
                            {"_InvFade", "Soft Factor"}
                        })
                }
            },
            #endregion
            #region hgforwardplanet
            {
                Resources.Load<Shader>("shaders/fx/hgforwardplanet"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Blending",
                        new Dictionary<string, string>()
                        {
                            {"_SrcBlendFloat", "Source Blend"},
                            {"_DstBlendFloat", "Destination Blend"},
                            {"_Cull", "Cull"}
                        }),
                    new ShaderPropertyCategory("Lighting and Base Colors",
                        new Dictionary<string, string>()
                        {
                            {"_TintColor", "Tint"},
                            {"_LightWarpRamp", "Lightwarp Ramp"},
                            {"_DetailStrength", "Detail Strength"},
                            {"_DiffuseTex", "Diffuse Texture"},
                            {"_DiffuseDetailTex", "Diffuse Detail Texture"},
                            {"_NormalStrength", "Normal Strength"},
                            {"_NormalTex", "Normal Map"},
                            {"_NormalDetailTex", "Normal Detail Map"},
                            {"_SpecColor", "Specular Color"},
                            {"_SpecularPower", "Specular Power"},
                            {"_DoubleSampleFactor", "Double Sample Factor"}
                        }),
                    new ShaderPropertyCategory("Atmosphere",
                        new Dictionary<string, string>()
                        {
                            {"_AtmosphereRamp", "Atmosphere Ramp"},
                            {"_AtmosphereStrength", "Atmosphere Strength"}
                        }),
                   new ShaderPropertyCategory("Emission",
                        new Dictionary<string, string>()
                        {
                            {"_EmissionTex", "Emission Texture"},
                            {"_EmissionRamp", "Emission Ramp"},
                            {"_EmissionStrength", "Emission Strength"}
                        }),
                   new ShaderPropertyCategory("Animation",
                        new Dictionary<string, string>()
                        {
                            {"_RotationSpeed", "Rotation Speed"}
                        })
                }
            },
            #endregion
            #region hggrass
            {
                Resources.Load<Shader>("shaders/environment/hgdistortion"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_Color", "Main Color"},
                            {"_MainTex", "Base (RGB)"},
                            {"_Scroll", "Scroll Speed (XY), Distortion Noise Scale (ZW)"},
                            {"_Cutoff", "Cutoff Alpha"},
                            {"_VertexOffsetStrength", "Vertex Offset Strength"},
                            {"_WindVector", "Wind Offset Vector"},
                            {"_Smoothness", "Smoothness"},
                            {"_RampInfo", "Ramp Choice"},
                            {"_SpecularStrength", "Specular Strength"},
                            {"_SpecularExponent", "Specular Exponent"}
                        })
                }
            },
            #endregion
            #region hgintersectioncloudremap
            {
                Resources.Load<Shader>("shaders/fx/hgintersectioncloudremap"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_SrcBlendFloat", "Source Blend"},
                            {"_DstBlendFloat", "Destination Blend"},
                            {"_TintColor", "Tint"},
                            {"_MainTex", "Base (RGB) Trans (A)"},
                            {"_Cloud1Tex", "Cloud 1 (RGB) Trans (A)"},
                            {"_Cloud2Tex", "Cloud 2 (RGB) Trans (A)"},
                            {"_RemapTex", "Color Remap Ramp (RGB)"},
                            {"_CutoffScroll", "Cutoff Scroll Speed"},
                            {"_InvFade", "Soft Factor"},
                            {"_SoftPower", "Soft Power"},
                            {"_Boost", "Brightness Boost"},
                            {"_RimPower", "Rim Power"},
                            {"_RimStrength", "Rim Strength"},
                            {"_AlphaBoost", "Alpha Boost"},
                            {"_IntersectionStrength", "Intersection Strength"},
                            {"_Cull", "Cull"},
                            {"_FadeFromVertexColorsOn", "Fade Alpha from Vertex Color Luminance"},
                            {"_TriplanarOn", "Enable Triplanar Projections for Clouds"}
                        })
                }
            },
            #endregion
            #region hgopaquecloudremap
            {
                Resources.Load<Shader>("shaders/fx/hgopaquecloudremap"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_TintColor", "Tint"},
                            {"_EmissionColor", "Emission"},
                            {"_MainTex", "Base (RGB) Trans (A)"},
                            {"_NormalStrength", "Normal Strength"},
                            {"_NormalTex", "Normal Map"},
                            {"_Cloud1Tex", "Cloud 1 (RGB) Trans (A)"},
                            {"_Cloud2Tex", "Cloud 2 (RGB) Trans (A)"},
                            {"_RemapTex", "Color Remap Ramp (RGB)"},
                            {"_CutoffScroll", "Cutoff Scroll Speed"},
                            {"_InvFade", "Soft Factor"},
                            {"_AlphaBoost", "Alpha Boost"},
                            {"_Cutoff", "Alpha Cutoff"},
                            {"_SpecularStrength", "Specular Strength"},
                            {"_SpecularExponent", "Specular Exponent"},
                            {"_ExtrusionStrength", "Extrusion Strength"},
                            {"_RampInfo", "Ramp Choice"},
                            {"_EmissionFromAlbedo", "Emission From Albedo"},
                            {"_CloudNormalMap", "Use NormalMap as a Cloud"},
                            {"_VertexAlphaOn", "Luminance for Vertex Alpha"},
                            {"_Cull", "Cull"}
                        })
                }
            },
            #endregion
            #region hgscopeshader
            {
                Resources.Load<Shader>("shaders/postprocess/hgscopeshader"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_ScopeMap", "Scope Distortion (R), Scope Tint (G)"},
                            {"_MainTex", ""},
                            {"_Scale", "Scale"},
                            {"_DistortionStrength", "Distortion Strength"},
                            {"_TintStrength", "Tint Strength"},
                        })
                }
            },
            #endregion
            #region hgscreendamage
            {
                Resources.Load<Shader>("shaders/postprocess/hgscreendamage"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_Tint", "Vignette Tint"},
                            {"_NormalMap", "Normal Map Texture"},
                            {"_TintStrength", "Vignette Strength"},
                            {"_DesaturationStrength", "Desaturation Strength"},
                            {"_DistortionStrength", "Distortion Strength"}
                        })
                }
            },
            #endregion
            #region hgsnowtopped
            {
                Resources.Load<Shader>("shaders/deferred/hgsnowtopped"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_Color", "Main Color"},
                            {"_MainTex", "Base (RGB)"},
                            {"_NormalStrength", "Normal Strength"},
                            {"_NormalTex", "Normal Map"},
                            {"_SnowTex", "Snow Texture (RGB) Depth (A)"},
                            {"_SnowNormalTex", "Snow Normal Map"},
                            {"_SnowBias", "Snow Y Bias"},
                            {"_Depth", "Blend Depth"},
                            {"_IgnoreBiasOn", "Ignore Alpha Weights"},
                            {"_BinaryBlendOn", "Blend Weights Binarily"},
                            {"_RampInfo", "Ramp Choice"},
                            {"_ForceSpecOn", "Ignore Diffuse Alpha for Speculars"},
                            {"_SpecularStrength", "Base Specular Strength"},
                            {"_SpecularExponent", "Base Specular Exponent"},
                            {"_Smoothness", "Base Smoothness"},
                            {"_SnowSpecularStrength", "Snow Specular Strength"},
                            {"_SnowSpecularExponent", "Snow Specular Exponent"},
                            {"_SnowSmoothness", "Snow Smoothness"},
                            {"_Fade", "Fade"}
                        }),
                    new ShaderPropertyCategory("Screenspace Dithering",
                        new Dictionary<string, string>()
                        {
                            {"_DitherOn", "Enable Dither"},
                        }),
                    new ShaderPropertyCategory("Triplanar Properties",
                        new Dictionary<string, string>()
                        {
                            {"_TriplanarOn", "Enable Snow Triplanar Projection"},
                            {"_TriplanarTextureFactor", "Triplanar Projection Texture Factor"},
                            {"_SnowOn", "Enable Snow Microfacets"},
                        }),
                    new ShaderPropertyCategory("WorldSpace Gradient Bias",
                        new Dictionary<string, string>()
                        {
                            {"_GradientBiasOn", "Enable World-Size Gradient Bias"},
                            {"_GradientBiasVector", "World-Size Gradient Bias Vector"},
                        }),
                    new ShaderPropertyCategory("Blue Channel Dirt",
                        new Dictionary<string, string>()
                        {
                            {"_DirtOn", "Enable Blue-Channel Dirt"},
                            {"_DirtTex", "Dirt Texture (RGB) Depth (A)"},
                            {"_DirtNormalTex", "Dirt Normal Map"},
                            {"_DirtBias", "Dirt Bias"},
                            {"_DirtSpecularStrength", "Dirt Specular Strength"},
                            {"_DirtSpecularExponent", "Dirt Specular Exponent"},
                            {"_DirtSmoothness", "Dirt Smoothness"}
                        }),
                }
            },
            #endregion
            #region hgsolidparallax
            {
                Resources.Load<Shader>("shaders/fx/hgsolidparallax"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_Color", "Color"},
                            {"_MainTex", "Albedo (RGB)"},
                            {"_EmissionTex", "Emission (RGB)"},
                            {"_EmissionPower", "Emission Power"},
                            {"_Normal", "Normal"},
                            {"_SpecularStrength", "Specular Strength"},
                            {"_SpecularExponent", "Specular Exponent"},
                            {"_Smoothness", "Smoothness"},
                            {"_Height1", "Height 1"},
                            {"_Height2", "Height 2"},
                            {"_HeightStrength", "Height Strength"},
                            {"_HeightBias", "Height Bias"},
                            {"_Parallax", "Parallax"},
                            {"_ScrollSpeed", "Height Scroll Speed"},
                            {"_RampInfo", "Ramp Choice"},
                            {"_Cull", "Cull"},
                            {"_ClipOn", "Alpha Clip"}
                        }),
                }
            },
            #endregion
            #region hgstandard
            {
                Resources.Load<Shader>("shaders/fx/hgstandard"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_EnableCutout", "Cutout"},
                            {"_Color", "Main Color"},
                            {"_MainTex", "Base (RGB) Specular Scale (A)"},
                            {"_NormalStrength", "Normal Strength"},
                            {"_NormalTex", "Normal Map"},
                            {"_EmColor", "Emission Color"},
                            {"_EmTex", "Emission Tex (RGB)"},
                            {"_EmPower", "Emission Power"},
                            {"_Smoothness", "Smoothness"},
                            {"_ForceSpecOn", "Ignore Diffuse Alpha for Speculars"},
                            {"_RampInfo", "Ramp Choice"},
                            {"_DecalLayer", "Decal Layer"},
                            {"_SpecularStrength", "Specular Strength"},
                            {"_SpecularExponent", "Specular Exponent"},
                            {"_Cull", "Cull"},
                        }),
                    new ShaderPropertyCategory("Screenspace Dithering",
                        new Dictionary<string, string>()
                        {
                            {"_DitherOn", "Enable Dither"},
                            {"_FadeBias", "Fade Bias"}
                        }),
                    new ShaderPropertyCategory("Fresnel Emission",
                        new Dictionary<string, string>()
                        {
                            {"_FEON", "Enable Fresnel Emission"},
                            {"_FresnelRamp", "Fresnel Ramp"},
                            {"_FresnelPower", "Fresnel Power"},
                            {"_FresnelMask", "Fresnel Mask"},
                            {"_FresnelBoost", "Fresnel Boost"},
                        }),
                    new ShaderPropertyCategory("Print Behavior",
                        new Dictionary<string, string>()
                        {
                            {"_PrintOn", "Enable Printing"},
                            {"_SliceHeight", "Slice Height"},
                            {"_SliceBandHeight", "Print Band Height"},
                            {"_SliceAlphaDepth", "Print Alpha Depth"},
                            {"_SliceAlphaTex", "Print Alpha Texture"},
                            {"_PrintBoost", "Print Color Boost"},
                            {"_PrintBias", "Print Alpha Bias"},
                            {"_PrintEmissionToAlbedoLerp", "Print Emission to Albedo Lerp"},
                            {"_PrintDirection", "Print Direction"},
                            {"_PrintRamp", "Print Ramp"},
                        }),
                    new ShaderPropertyCategory("Elite Remap Behavior",
                        new Dictionary<string, string>()
                        {
                            {"_EliteBrightnessMin", "Elite Brightness, Min"},
                            {"_EliteBrightnessMax", "Elite Brightness, Max"}
                        }),
                    new ShaderPropertyCategory("Splatmapping",
                        new Dictionary<string, string>()
                        {
                            {"_SplatmapOn", "Enable Splatmap"},
                            {"_ColorsOn", "Use Vertex Colors Instead"},
                            {"_Depth", "Blend Depth"},
                            {"_SplatmapTex", "Splatmap Tex (RGB)"},
                            {"_SplatmapTileScale", "Splatmap Tile Scale"},
                            {"_GreenChannelTex", "Green Channel Albedo (RGB) Specular Scale (A)"},
                            {"_GreenChannelNormalTex", "Green Channel Normal Map"},
                            {"_GreenChannelSmoothness", "Green Channel Smoothness"},
                            {"_GreenChannelBias", "Green Channel Bias"},
                            {"_BlueChannelTex", "Blue Channel Albedo  Specular Scale (A)"},
                            {"_BlueChannelNormalTex", "Blue Channel Normal Map"},
                            {"_BlueChannelSmoothness", "Blue Channel Smoothness"},
                            {"_BlueChannelBias", "Blue Channel Bias"},
                        }),
                    new ShaderPropertyCategory("Flowmap",
                        new Dictionary<string, string>()
                        {
                            {"_FlowmapOn", "Enable Flowmap"},
                            {"_FlowTex", "Flow Vector (RG), Noise (B)"},
                            {"_FlowHeightmap", "Flow Heightmap"},
                            {"_FlowHeightRamp", "Flow Ramp"},
                            {"_FlowHeightBias", "Flow Height Bias"},
                            {"_FlowHeightPower", "Flow Height Power"},
                            {"_FlowEmissionStrength", "Flow Height Strength"},
                            {"_FlowSpeed", "Flow Speed"},
                            {"_FlowMaskStrength", "Mask Flow Strength"},
                            {"_FlowNormalStrength", "Normal Flow Strength"},
                            {"_FlowTextureScaleFactor", "Flow Texture Scale Factor"},
                        }),
                    new ShaderPropertyCategory("Limb Removal",
                        new Dictionary<string, string>()
                        {
                            {"_LimbRemovalOn", "Enable Limb Removal"}
                        }),
                }
            },
            #endregion
            #region hgtriplanarterrainplainblend
            {
                Resources.Load<Shader>("shaders/deferred/hgtriplanarterrainplainblend"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_ColorsOn", "Use Vertex Colors Instead"},
                            {"_MixColorsOn", "Mix Vertex Colors with Texture"},
                            {"_MaskOn", "Use Alpha Channels as Weight Mask"},
                            {"_VerticalBiasOn", "Bias Green Channel to Vertical"},
                            {"_DoublesampleOn", "Double Sample UVs"},
                            {"_Color", "Main Color"},
                            {"_NormalTex", "Normal Tex (RGB)"},
                            {"_NormalStrength", "Normal Strength"},
                            {"_RampInfo", "Ramp Choice"},
                            {"_DecalLayer", "Decal Layer"},
                            {"_Cull", "Cull"},
                            {"_TextureFactor", "Texture Factor"},
                            {"_Depth", "Blend Depth"},
                            {"_SplatmapTex", "Splatmap Tex (RGB)"},
                            {"_RedChannelTopTex", "Red Channel Top Albedo (RGB) Specular Scale (A)"},
                            {"_RedChannelSideTex", "Red Channel Side Albedo (RGB) Specular Scale (A)"},
                            {"_RedChannelSmoothness", "Red Channel Smoothness"},
                            {"_RedChannelSpecularStrength", "Red Channel Specular Strength"},
                            {"_RedChannelSpecularExponent", "Red Channel Specular Exponent"},
                            {"_RedChannelBias", "Red Channel Bias"},
                            {"_GreenChannelTex", "Green Channel Albedo (RGB) Specular Scale (A)"},
                            {"_GreenChannelSmoothness", "Green Channel Smoothness"},
                            {"_GreenChannelSpecularStrength", "Green Channel Specular Strength"},
                            {"_GreenChannelSpecularExponent", "Green Channel Specular Exponent"},
                            {"_GreenChannelBias", "Green Channel Bias"},
                            {"_BlueChannelTex", "Blue Channel Albedo  Specular Scale (A)"},
                            {"_BlueChannelSmoothness", "Blue Channel Smoothness"},
                            {"_BlueChannelSpecularStrength", "Blue Channel Specular Strength"},
                            {"_BlueChannelSpecularExponent", "Blue Channel Specular Exponent"},
                            {"_BlueChannelBias", "Blue Channel Bias"},
                            {"_SnowOn", "Treat G.Channel as Snow"}
                        })
                }
            },
            #endregion
            #region hguianimatealpha
            {
                Resources.Load<Shader>("shaders/ui/hguianimatealpha"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_RemapRamp", "Remap Ramp"},
                            {"_AlphaBoost", "Alpha Boost"},
                            {"_LightenFactor", "Alpha Lighten Factor"},
                            {"_TrueAlpha", "True Alpha"},
                            {"_OverlayTex", "Overlay Texture"},
                            {"_PatternPixelSize", "Pattern Pixel Size"},
                            {"_PatternStrength", "Pattern Strength"},
                            {"_PatternPanningSpeed", "Pattern Panning Speed"},
                            {"_DoubleSampleOn", "Double Sample Pattern"},
                            {"_Color", "Tint"},
                            {"_StencilComp", "Stencil Comparison"},
                            {"_Stencil", "Stencil ID"},
                            {"_StencilOp", "Stencil Operation"},
                            {"_StencilWriteMask", "Stencil Write Mask"},
                            {"_StencilReadMask", "Stencil Read Mask"},
                            {"_ColorMask", "Color Mask"},
                            {"_UseUIAlphaClip", "Use Alpha Clip"},
                        })
                }
            },
            #endregion
            #region hguibarremap
            {
                Resources.Load<Shader>("shaders/ui/hguibarremap"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_MainTex", "Gradient (R) Mask (G)"},
                            {"_RemapTex", "Color Remap Ramp (RGB)"},
                            {"_GradientScale", "Gradient Scale"},
                            {"_PingPong", "PingPong Ramp"}
                        })
                }
            },
            #endregion
            #region hguiblur
            {
                Resources.Load<Shader>("shaders/ui/hguiblur"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_Radius", "Radius"}
                        })
                }
            },
            #endregion
            #region hguicustomblend
            {
                Resources.Load<Shader>("shaders/ui/hguicustomblend"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_Color", "Tint"},
                            {"_StencilComp", "Stencil Comparison"},
                            {"_Stencil", "Stencil ID"},
                            {"_StencilOp", "Stencil Operation"},
                            {"_StencilWriteMask", "Stencil Write Mask"},
                            {"_StencilReadMask", "Stencil Read Mask"},
                            {"_ColorMask", "Color Mask"},
                            {"_UseUIAlphaClip", "Use Alpha Clip"},
                        })
                }
            },
            #endregion
            #region hguiignorez
            {
                Resources.Load<Shader>("shaders/ui/hguiignorez"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_MainTex", "Texture"},
                        })
                }
            },
            #endregion
            #region hguioverbrighten
            {
                Resources.Load<Shader>("shaders/ui/hguioverbrighten"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_Color", "Tint"},
                            {"_OverbrightenScale", "Overbrighten Scale"},
                            {"_StencilComp", "Stencil Comparison"},
                            {"_Stencil", "Stencil ID"},
                            {"_StencilOp", "Stencil Operation"},
                            {"_StencilWriteMask", "Stencil Write Mask"},
                            {"_StencilReadMask", "Stencil Read Mask"},
                            {"_ColorMask", "Color Mask"},
                            {"_UseUIAlphaClip", "Use Alpha Clip"},
                        })
                }
            },
            #endregion
            #region hgvertexonly
            {
                Resources.Load<Shader>("shaders/fx/hgvertexonly"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_TintColor", "AAH"}
                        })
                }
            },
            #endregion
            #region hgwaterfall
            {
                Resources.Load<Shader>("shaders/environment/hgwaterfall"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_Color", "Main Color"},
                            {"_MainTex", "Base (RGB)"},
                            {"_NormalTex", "Normal Map"},
                            {"_NormalStrength", "Normal Strength"},
                            {"_VertexOffsetStrength", "Vertex Offset Strength"},
                            {"_PerlinScale", "Perlin Scale"},
                            {"_Cutoff", "Alpha Cutoff"},
                            {"_FoamTex", "Foam Texture"},
                            {"_Depth", "Blend Depth"},
                            {"_Speed", "Water Speed"},
                            {"_Smoothness", "Smoothness"},
                            {"_RampInfo", "Ramp Choice"},
                            {"_SpecularStrength", "Specular Strength"},
                            {"_SpecularExponent", "Specular Exponent"}
                        })
                }
            },
            #endregion
            #region hgwavycloth
            {
                Resources.Load<Shader>("shaders/deferred/hgwavycloth"),
                new ShaderPropertyCategory[]
                {
                    new ShaderPropertyCategory("Default",
                        new Dictionary<string, string>()
                        {
                            {"_Color", "Main Color"},
                            {"_Cutoff", "Alpha Cutoff Value"},
                            {"_MainTex", "Base (RGB)"},
                            {"_ScrollingNormalMap", "Scrolling Normal Map"},
                            {"_NormalStrength", "Normal Strength"},
                            {"_Scroll", "Scroll Speed (XY), Distortion Noise Scale (ZW)"},
                            {"_VertexOffsetStrength", "Vertex Offset Strength"},
                            {"_WindVector", "Wind Offset Vector"},
                            {"_Smoothness", "Smoothness"},
                            {"_RampInfo", "Ramp Choice"},
                            {"_SpecularStrength", "Specular Strength"},
                            {"_SpecularExponent", "Specular Exponent"},
                            {"_EnableVertexColorDistortion", "Use Vertex R channel for distortion strength"},
                        })
                }
            },
            #endregion
        };


        void Start()
        {

        }

        void Update()
        {

        }
    }

    public struct ShaderPropertyCategory
    {
        public string categoryName;
        public Dictionary<string, string> propertyToUserFacingName;

        public ShaderPropertyCategory(string name, Dictionary<string, string> userFacingData)
        {
            categoryName = name;
            propertyToUserFacingName = userFacingData;
        }
    }
}
#endif
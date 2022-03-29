Shader "StubbedDecalicious/Deferred Decal"
{
	Properties
	{
	_MaskTex("Mask", 2D) = "white" { }
	_MaskMultiplier("Mask (Multiplier)", Float) = 1
	_MaskNormals("Mask Normals?", Float) = 1
	_LimitTo("Limit To", Float) = 0
	_MainTex("Albedo", 2D) = "white" { }
	_Color("Albedo (Multiplier)", Color) = (1,1,1,1)
	_NormalTex("Normal", 2D) = "bump" { }
	_NormalMultiplier("Normal (Multiplier)", Float) = 1
	_SpecularStrength("Specular Strength", Range(0, 1)) = 0
	_SpecularExponent("Specular Exponent", Range(0.1, 20)) = 1
	_Smoothness("Smoothness (Multiplier)", Range(0, 1)) = 0.5
	[MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo("Ramp Choice", Float) = 0
	[MaterialEnum(Character,0,Environment,1,Misc,2)] _DecalLayer("Decal Layer", Float) = 0
	_DecalBlendMode("Blend Mode", Float) = 0
	_DecalSrcBlend("SrcBlend", Float) = 1
	_DecalDstBlend("DstBlend", Float) = 10
	_NormalBlendMode("Normal Blend Mode", Float) = 0
	_AngleLimit("Angle Limit", Float) = 0.5
	_CloudOn("Use Cloud Remap", Float) = 0
	_Cloud1Tex("Cloud 1 (RGB) Trans (A)", 2D) = "grey" { }
	_Cloud2Tex("Cloud 2 (RGB) Trans (A)", 2D) = "grey" { }
	_RemapTex("Color Remap Ramp (RGB)", 2D) = "grey" { }
	_CutoffScroll("Cutoff Scroll Speed", Vector) = (0,0,0,0)
	_AlphaBoost("Alpha Boost", Range(0, 20)) = 1
	}
}

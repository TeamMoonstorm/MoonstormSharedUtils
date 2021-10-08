Shader "StubbedShader/ui/hguianimatealpha" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _ExternalAlpha ("External Alpha", Range(0, 1)) = 1
		_RemapRamp ("Remap Ramp", 2D) = "white" {}
		_AlphaBoost ("Alpha Boost", Range(0, 20)) = 1
		_LightenFactor ("Alpha Lighten Factor", Range(0, 1)) = 0
		_TrueAlpha ("True Alpha", Range(0, 1)) = 1
		_OverlayTex ("Overlay Texture", 2D) = "white" {}
		_PatternPixelSize ("Pattern Pixel Size", Range(0, 512)) = 32
		_PatternStrength ("Pattern Strength", Range(0, 1)) = 1
		_PatternPanningSpeed ("Pattern Panning Speed", Range(0, 20)) = 0
		[Toggle(DOUBLESAMPLE)] _DoubleSampleOn ("Double Sample Pattern", Float) = 0
		_Color ("Tint", Color) = (1,1,1,1)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}
		Fallback "Diffuse"
}
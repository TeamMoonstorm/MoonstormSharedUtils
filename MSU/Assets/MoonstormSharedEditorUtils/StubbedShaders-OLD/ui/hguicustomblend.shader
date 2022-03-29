Shader "StubbedShader/ui/hguicustomblend" {
	Properties {
		[HideInInspector] _SrcBlend ("Source Blend", Float) = 1
		[HideInInspector] _DstBlend ("Destination Blend", Float) = 1
		[HideInInspector] _InternalSimpleBlendMode ("Internal Simple Blend Mode", Float) = 0
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[HDR] _Color ("Tint", Color) = (1,1,1,1)
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
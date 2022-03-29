Shader "StubbedShader/environment/hgwaterfall" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_NormalTex ("Normal Map", 2D) = "bump" {}
		_NormalStrength ("Normal Strength", Range(0, 1)) = 1
		_VertexOffsetStrength ("Vertex Offset Strength", Range(0, 5)) = 1
		_PerlinScale ("Perlin Scale", Vector) = (0,0,0,0)
		_Cutoff ("Alpha Cutoff", Range(0, 2)) = 0.5
		_FoamTex ("Foam Texture", 2D) = "white" {}
		_Depth ("Blend Depth", Range(0, 1)) = 0.2
		_Speed ("Water Speed", Range(-5, 5)) = 0.1
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		[MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo ("Ramp Choice", Float) = 0
		_SpecularStrength ("Specular Strength", Range(0, 1)) = 0
		_SpecularExponent ("Specular Exponent", Range(0, 20)) = 1
	}
	Fallback "Diffuse"
}
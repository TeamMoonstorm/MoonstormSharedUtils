Shader "StubbedShader/environment/hgdistantwater" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_NormalStrength ("Normal Strength", Range(0, 5)) = 1
		_Normal1Tex ("Normal Map 1", 2D) = "bump" {}
		_Normal2Tex ("Normal Map 2", 2D) = "bump" {}
		_Scroll ("Scroll Speed", Vector) = (0,0,0,0)
		_VertexOffsetStrength ("Vertex Offset Strength", Range(0, 5)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		[MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo ("Ramp Choice", Float) = 0
		_SpecularStrength ("Specular Strength", Range(0, 1)) = 0
		_SpecularExponent ("Specular Exponent", Range(0, 20)) = 1
	}
	Fallback "Diffuse"
}
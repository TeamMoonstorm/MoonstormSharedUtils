Shader "StubbedShader/environment/hggrass" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Scroll ("Scroll Speed (XY), Distortion Noise Scale (ZW)", Vector) = (0,0,0,0)
		_Cutoff ("Cutoff Alpha", Range(0, 1)) = 0.5
		_VertexOffsetStrength ("Vertex Offset Strength", Range(0, 5)) = 0
		_WindVector ("Wind Offset Vector", Vector) = (0,0,0,0)
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		[MaterialEnum(Two Tone,0,Smoothed Two Tone,1,Unlitish,3,Subsurface,4,Grass,5)] _RampInfo ("Ramp Choice", Float) = 0
		_SpecularStrength ("Specular Strength", Range(0, 1)) = 0
		_SpecularExponent ("Specular Exponent", Range(0, 20)) = 1
	}
	Fallback "Diffuse"
}
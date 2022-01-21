Shader "StubbedShader/postprocess/hgscopeshader" {
	Properties {
		_ScopeMap ("Scope Distortion (R), Scope Tint (G)", 2D) = "white" {}
		[HideInInspector] _MainTex ("", any) = "" {}
		_Scale ("Scale", Range(1, 10)) = 1
		_DistortionStrength ("Distortion Strength", Range(-1, 1)) = 1
		_TintStrength ("Tint Strength", Range(0, 1)) = 0.5
	}
		Fallback "Diffuse"
}
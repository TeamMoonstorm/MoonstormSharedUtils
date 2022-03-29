Shader "StubbedShader/fx/hgdamagenumber" {
	Properties {
		[HDR] _TintColor ("Tint", Color) = (1,1,1,1)
		_CritColor ("Crit Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_CharacterLimit ("Character Limit", Float) = 3
	}
		Fallback "Diffuse"
}
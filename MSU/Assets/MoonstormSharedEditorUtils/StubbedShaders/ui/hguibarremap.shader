Shader "StubbedShader/ui/hguibarremap" {
	Properties {
		_MainTex ("Gradient (R) Mask (G)", 2D) = "grey" {}
		_RemapTex ("Color Remap Ramp (RGB)", 2D) = "grey" {}
		_GradientScale ("Gradient Scale", Range(0, 1.5)) = 1
		[Toggle(PINGPONG)] _PingPong ("PingPong Ramp", Float) = 0
	}
	Fallback "Transparent/VertexLit"
}
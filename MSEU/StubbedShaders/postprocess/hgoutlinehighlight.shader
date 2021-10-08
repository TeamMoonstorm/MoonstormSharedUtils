Shader "StubbedShader/postprocess/hgoutlinehighlight" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "black" {}
		_OutlineMap ("Occlusion Map", 2D) = "black" {}
	}
		Fallback "Diffuse"
}
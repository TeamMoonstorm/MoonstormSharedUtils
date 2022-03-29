Shader "StubbedShader/postprocess/hgsobelbuffer" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "black" {}
	}
		Fallback "Diffuse"
}
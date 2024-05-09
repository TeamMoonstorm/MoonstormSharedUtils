using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MSU.Editor.EditorWindows
{
    public class AddressableMaterialPreview : EditorWindow
    {
        public string materialAddress;
        PreviewRenderUtility previewRender;
        Texture outputTexture;
        Material material;
        private void Preview()
        {
            if (previewRender != null)
                previewRender.Cleanup();
            previewRender = new PreviewRenderUtility(true);
            System.GC.SuppressFinalize(previewRender);
            var cam = previewRender.camera;
            cam.fieldOfView = 30f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            cam.transform.position = new Vector3(2.5f, 1f, -2.5f);
            cam.transform.LookAt(Vector3.zero);

            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.GetComponent<MeshRenderer>().material = material;
            outputTexture = CreatePreviewTexture(obj);
            DestroyImmediate(obj);
        }

        private RenderTexture CreatePreviewTexture(GameObject obj)
        {
            previewRender.BeginPreview(new Rect(0, 0, 128, 128), GUIStyle.none);

            previewRender.lights[0].transform.localEulerAngles = new Vector3(30, 30, 0);
            previewRender.lights[0].intensity = 2;
            previewRender.AddSingleGO(obj);
            previewRender.camera.Render();

            return (RenderTexture)previewRender.EndPreview();
        }
        void OnGUI()
        {
            if (material == null)
            {
                material = Addressables.LoadAssetAsync<Material>(materialAddress).WaitForCompletion();
            }

            if (outputTexture == null)
            {
                Preview();
            }
            GUI.DrawTexture(new Rect(0, 0, 300, 300), outputTexture);
        }

        void OnDisable()
        {
            previewRender.Cleanup();
        }
    }
}
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MSU.Editor.EditorWindows
{
    public class AddressableMaterialPreview : EditorWindow
    {
        public string materialAddress;
        private PreviewRenderUtility _previewRender;
        private Texture _outputTexture;
        private Material _material;
        private void Preview()
        {
            if (_previewRender != null)
                _previewRender.Cleanup();
            _previewRender = new PreviewRenderUtility(true);
            System.GC.SuppressFinalize(_previewRender);
            var cam = _previewRender.camera;
            cam.fieldOfView = 30f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            cam.transform.position = new Vector3(2.5f, 1f, -2.5f);
            cam.transform.LookAt(Vector3.zero);

            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.GetComponent<MeshRenderer>().material = _material;
            _outputTexture = CreatePreviewTexture(obj);
            DestroyImmediate(obj);
        }

        private RenderTexture CreatePreviewTexture(GameObject obj)
        {
            _previewRender.BeginPreview(new Rect(0, 0, 128, 128), GUIStyle.none);

            _previewRender.lights[0].transform.localEulerAngles = new Vector3(30, 30, 0);
            _previewRender.lights[0].intensity = 2;
            _previewRender.AddSingleGO(obj);
            _previewRender.camera.Render();

            return (RenderTexture)_previewRender.EndPreview();
        }
        void OnGUI()
        {
            if (_material == null)
            {
                _material = Addressables.LoadAssetAsync<Material>(materialAddress).WaitForCompletion();
            }

            if (_outputTexture == null)
            {
                Preview();
            }
            GUI.DrawTexture(new Rect(0, 0, 300, 300), _outputTexture);
        }

        void OnDisable()
        {
            _previewRender.Cleanup();
        }
    }
}
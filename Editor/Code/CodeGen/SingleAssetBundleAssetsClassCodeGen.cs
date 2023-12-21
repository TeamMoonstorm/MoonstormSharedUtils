using RoR2EditorKit.CodeGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.Editor.CodeGen
{
    public class SingleAssetBundleAssetsClassCodeGen
    {
        private Writer writer;
        private Data data;

        public Writer WriteCode(out Output output)
        {
            var fileName = Path.GetFileNameWithoutExtension(data.path);
            var className = CSharpCodeHelpers.MakeIdentifier(fileName);
            output = new Output
            {
                assetbundleClassName = className,
            };

            CodeGenUtil.WriteHeader(writer, "SingleAssetBundleAssetsClassCodeGen", "1.0.0");

            WriteUsings();
            writer.WriteLine();
            writer.WriteLine($"namespace {data.modName}");
            writer.BeginBlock();

            writer.WriteLine($"internal static class {className}");
            writer.BeginBlock();

            writer.WriteLine("public static AssetBundle MainAssetBundle { get; private set; }");
            writer.WriteLine($"private static string AssetBundlePath => Path.Combine({data.mainClassName}.PluginInfo.Location, \"assetbundles\", \"{data.assetbundleName}\");");
            writer.WriteLine();
            writer.WriteLine("private static List<Material> _swappedShaderMaterials = new List<Material>();");
            writer.WriteLine();
            writer.WriteLine("public static TAsset LoadAsset<TAsset>(string name) where TAsset : UnityEngine.Object");
            writer.BeginBlock();

            writer.WriteLine("return MainAssetBundle.LoadAsset<TAsset>(name);");
            writer.EndBlock();
            writer.WriteLine();
            writer.WriteLine("public static AssetBundleRequest LoadAssetAsync<TAsset>(string name) where TAsset : UnityEngine.Object");
            writer.BeginBlock();

            writer.WriteLine("return MainAssetBundle.LoadAssetAsync<TAsset>(name);");
            writer.EndBlock();
            writer.WriteLine();
            writer.WriteLine("public static TAsset[] LoadAllAssets<TAsset>() where TAsset : UnityEngine.Object");
            writer.BeginBlock();

            writer.WriteLine("return MainAssetBundle.LoadAllAssets<TAsset>();");
            writer.EndBlock();
            writer.WriteLine();
            writer.WriteLine("public static AssetBundleRequest LoadAllAssetsAsync<TAsset>() where TAsset : UnityEngine.Object");
            writer.BeginBlock();

            writer.WriteLine("return MainAssetBundle.LoadAllAssetsAsync<TAsset>();");
            writer.EndBlock();
            writer.WriteLine("//This method gets called during ContentPack loading, which will ensure assets load asynchronously properly.");
            WriteInitializeMethod();

            writer.WriteLine();
            writer.WriteLine("//This method loads our main assetbundle into memory");
            WriteLoadAssetBundle();

            writer.WriteLine();
            writer.WriteLine("//Swaps the stubbed shaders in your assetbundles.");
            WriteSwapShaders();

            writer.WriteLine();
            writer.WriteLine("//Swaps Materials with AddressableMaterial shaders.");
            WriteSwapAddressableShaders();

            writer.EndBlock();
            writer.EndBlock();
            return writer;
        }

        private void WriteUsings()
        {
            writer.WriteLine(
@"using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using RoR2;
using Path = System.IO.Path;");
        }

        private void WriteInitializeMethod()
        {

            writer.WriteLine("internal static IEnumerator Initialize(LoadStaticContentAsyncArgs args)");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Message(\"Initializing Assets...\");");
            writer.WriteLine("yield return LoadAssetBundle(args);");
            writer.WriteLine("yield return SwapShaders(args);");
            writer.WriteLine("yield return SwapAddressableShaders(args);");
            writer.EndBlock();
        }

        private void WriteLoadAssetBundle()
        {
            writer.WriteLine("private static IEnumerator LoadAssetBundle(LoadStaticContentAsyncArgs args)");
            writer.BeginBlock();

            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine($"{data.loggerClassName}.Debug($\"Loading assetbundle located at \\\"{{AssetBundlePath}}\\\"\");");
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("var bundleLoadRequest = AssetBundle.LoadFromFileAsync(AssetBundlePath);");
            writer.WriteLine("while(!bundleLoadRequest.isDone)");
            writer.BeginBlock();

            writer.WriteLine("args.ReportProgress(Util.Remap(bundleLoadRequest.progress + 1, 0f, 1, 0f, 0.2f));");
            writer.WriteLine("yield return null;");
            writer.EndBlock();

            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine($"{data.loggerClassName}.Debug(\"Loaded main assetbundle.\");");
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("MainAssetBundle = bundleLoadRequest.assetBundle;");
            writer.EndBlock();
        }

        private void WriteSwapShaders()
        {
            writer.WriteLine("private static IEnumerator SwapShaders(LoadStaticContentAsyncArgs args)");
            writer.BeginBlock();

            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine($"{data.loggerClassName}.Debug(\"Swapping stubbbed shaders from main assetbundle.\");");
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("var assetRequest = MainAssetBundle.LoadAllAssetsAsync<Material>();");
            writer.WriteLine("while(!assetRequest.isDone)");
            writer.BeginBlock();

            writer.WriteLine("args.ReportProgress(Util.Remap(assetRequest.progress, 0f, 1f, 0.2f, 0.3f));");
            writer.WriteLine("yield return null;");
            writer.EndBlock();

            writer.WriteLine("var materials = assetRequest.allAssets.OfType<Material>().Where(mat => mat.shader.name.StartsWith(\"Stubbed\")).ToArray();");
            writer.WriteLine("for(int i = 0; i < materials.Length; i++)");
            writer.BeginBlock();

            writer.WriteLine("var material = materials[i];");
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine($"{data.loggerClassName}.Debug($\"Swapping the shader of material \\\"{{material}}\\\"\");");
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("AsyncOperationHandle<Shader> asyncOp = default;");
            writer.WriteLine("try");
            writer.BeginBlock();

            writer.WriteLine("var shaderName = material.shader.name.Substring(\"Stubbed\".Length);");
            writer.WriteLine("var addressablePath = $\"{shaderName}.shader\";");
            writer.WriteLine("asyncOp = Addressables.LoadAssetAsync<Shader>(addressablePath);");
            writer.EndBlock();

            writer.WriteLine("catch(Exception e)");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Error($\"Failed to swap the shader of material \\\"{{material}}\\\". {{e}}\");");
            writer.WriteLine("continue;");
            writer.EndBlock();

            writer.WriteLine("while(!asyncOp.IsDone)");
            writer.BeginBlock();

            writer.WriteLine("args.ReportProgress(RoR2.Util.Remap(asyncOp.PercentComplete + i, 0f, 1f, 0.3f, 0.4f));");
            writer.WriteLine("yield return null;");
            writer.EndBlock();

            writer.WriteLine("material.shader = asyncOp.Result;");
            writer.WriteLine("_swappedShaderMaterials.Add(material);");
            writer.EndBlock();
            writer.EndBlock();
        }

        private void WriteSwapAddressableShaders()
        {
            writer.WriteLine("private static IEnumerator SwapAddressableShaders(LoadStaticContentAsyncArgs args)");
            writer.BeginBlock();


            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine($"{data.loggerClassName}.Debug(\"Finalizing materials with AddressableMaterial shaders from main assetbundle.\");");
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("var assetRequest = MainAssetBundle.LoadAllAssetsAsync<Material>();");
            writer.WriteLine("while(!assetRequest.isDone)");
            writer.BeginBlock();

            writer.WriteLine("args.ReportProgress(Util.Remap(assetRequest.progress, 0f, 1f, 0.4f, 0.6f));");
            writer.WriteLine("yield return null;");
            writer.EndBlock();

            writer.WriteLine();
            writer.WriteLine("var materials = assetRequest.allAssets.OfType<Material>().Where(mat => mat.shader.name == \"AddressableMaterialShader\").ToArray();");
            writer.WriteLine("for(int i = 0; i < materials.Length; i++)");
            writer.BeginBlock();

            writer.WriteLine("var material = materials[i];");
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine($"{data.loggerClassName}.Debug($\"Finalizing material {{material}}\");");
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("AsyncOperationHandle<Material> asyncOp = default;");
            writer.WriteLine("try");
            writer.BeginBlock();

            writer.WriteLine("var shaderKeywords = material.shaderKeywords;");
            writer.WriteLine("var address = shaderKeywords[0];");
            writer.WriteLine("asyncOp = Addressables.LoadAssetAsync<Material>(address);");
            writer.EndBlock();

            writer.WriteLine("catch(Exception e)");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Error($\"Failed to finalize the material \\\"{{material}}\\\", {{e}}\");");
            writer.WriteLine("continue;");
            writer.EndBlock();

            writer.WriteLine();
            writer.WriteLine("while(!asyncOp.IsDone)");
            writer.BeginBlock();

            writer.WriteLine("args.ReportProgress(Util.Remap(asyncOp.PercentComplete + i, 0f, 1f, 0.6f, 0.8f));");
            writer.WriteLine("yield return null;");
            writer.EndBlock();

            writer.WriteLine();
            writer.WriteLine("var loadedMat = asyncOp.Result;");
            writer.WriteLine("material.shader = loadedMat.shader;");
            writer.WriteLine("material.CopyPropertiesFromMaterial(loadedMat);");
            writer.WriteLine("_swappedShaderMaterials.Add(material);");
            writer.EndBlock();
            writer.EndBlock();
        }

        public SingleAssetBundleAssetsClassCodeGen(Data data)
        {
            writer = new Writer
            {
                buffer = new StringBuilder()
            };
            this.data = data;
        }

        public struct Data
        {
            public string modName;
            public string mainClassName;
            public string loggerClassName;
            public string assetbundleName;

            public string path;
        }

        public struct Output
        {
            public string assetbundleClassName;
        }
    }
}

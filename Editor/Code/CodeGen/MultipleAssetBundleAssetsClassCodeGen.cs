using RoR2.ContentManagement;
using RoR2EditorKit.CodeGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.Editor.CodeGen
{
    public class MultipleAssetBundleAssetsClassCodeGen
    {
        private Writer writer;
        private Data data;
        private Output _output;
        public Writer WriteCode(out Output output)
        {
            var fileName = Path.GetFileNameWithoutExtension(data.path);
            var className = CSharpCodeHelpers.MakeIdentifier(fileName);
            _output = new Output
            {
                assetBundleClassName = className,
            };

            CodeGenUtil.WriteHeader(writer, "SingleAssetBundleAssetsClassCodeGen", "1.0.0");

            writer.WriteLine();
            WriteUsings();
            writer.WriteLine();
            writer.WriteLine($"namespace {data.modName}");
            writer.BeginBlock();

            _output.bundleEnumName = $"{data.modName}Bundle";
            WriteBundleEnum();
            writer.WriteLine();
            writer.WriteLine("//This is your assets class. it'll contain all of your mod's assetbundles.");
            writer.WriteLine($"internal static class {className}");
            writer.BeginBlock();

            writer.WriteLine("private const string ASSET_BUNDLE_FOLDER_NAME = \"assetbundles\";");
            writer.WriteLine();
            writer.WriteLine("//Try to keep a specific constant for each of your non streamed scene bundles.");
            writer.WriteLine($"private const string MAIN = \"{data.assetBundleName}\";");
            writer.WriteLine();
            writer.WriteLine($"private static string AssetBundleFolderPath => Path.Combine({data.mainClassName}.PluginInfo.Location, ASSET_BUNDLE_FOLDER_NAME);");
            writer.WriteLine($"private static Dictionary<{_output.bundleEnumName}, AssetBundle> _assetBundles = new Dictionary<{_output.bundleEnumName}, AssetBundle>();");
            writer.WriteLine("private static AssetBundle[] _streamedSceneBundles = Array.Empty<AssetBundle>();");
            writer.WriteLine("private static List<Material> _swappedShaderMaterials = new List<Material>();");
            writer.WriteLine();
            writer.WriteLine($"public static AssetBundle GetAssetBundle({_output.bundleEnumName} bundle) => _assetBundles[bundle];");
            writer.WriteLine();
            WriteLoadAssetMethod();
            writer.WriteLine();
            writer.WriteLine("public static IEnumerator LoadAssetAsync<TAsset>(SingleBundleAssetRequest<TAsset> request) where TAsset : UnityEngine.Object");
            writer.BeginBlock();
            writer.WriteLine("yield return request.Load();");
            writer.EndBlock();
            writer.WriteLine();
            WriteLoadAllAssetsMethod();
            writer.WriteLine();
            writer.WriteLine("public static IEnumerator LoadAllAssetsAsync<TAsset>(MultipleBundleAssetRequest<TAsset> request) where TAsset : UnityEngine.Object");
            writer.BeginBlock();
            writer.WriteLine("yield return request.Load();");
            writer.EndBlock();
            writer.WriteLine();
            writer.WriteLine("//This method gets called during ContentPack loading, which will ensure assets load asynchronously properly.");
            WriteInitializeMethod();
            writer.WriteLine();
            WriteLoadAssetBundlesMethod();
            writer.WriteLine();
            writer.WriteLine("private static string[] GetAssetBundlePaths() => Directory.GetFiles(AssetBundleFolderPath).Where(filePath => !filePath.EndsWith(\".manifest\")).ToArray();");
            writer.WriteLine();
            WriteSwapShaders();
            writer.WriteLine();
            WriteSwapAddressableShaders();
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            WriteGetCallingMethod();
            writer.WriteLine();
            WriteGetMethodParams();
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine();
            WriteSingleBundleAssetRequest();
            writer.WriteLine();
            WriteMultipleBundleAssetRequest();
            writer.WriteLine();
            writer.EndBlock();
            writer.EndBlock();
            output = _output;
            return writer;
        }

        private void WriteUsings()
        {
            writer.WriteLine(
@"using System;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Collections;
using RoR2.ContentManagement;
using RoR2;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Path = System.IO.Path;");
        }

        private void WriteBundleEnum()
        {
            writer.WriteLine("//These enums represent a specific, non streamed scene bundle in your project. The exceptions are \"Invalid\", which represents an Invalid assetbundle, and the special enum \"All\", which is used to do a global search.");
            writer.WriteLine($"public enum {data.modName}Bundle");
            writer.BeginBlock();
            writer.WriteLine("Invalid,");
            writer.WriteLine("All,");
            writer.WriteLine("Main");
            writer.EndBlock();
        }

        private void WriteLoadAssetMethod()
        {
            writer.WriteLine($"public static TAsset LoadAsset<TAsset>(string name, {_output.bundleEnumName} bundle) where TAsset : UnityEngine.Object");
            writer.BeginBlock();
            writer.WriteLine("TAsset asset = null;");
            writer.WriteLine($"if(bundle == {_output.bundleEnumName}.All)");
            writer.BeginBlock();

            writer.WriteLine($"asset = FindAsset(name, out {_output.bundleEnumName} foundInBundle);");
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine("if(!asset)");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Warning($\"Could not find asset of type {{typeof(TAsset).Name}} with name {{name}} in any of the bundles.\");");
            writer.EndBlock();

            writer.WriteLine("else");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Info($\"Asset of type {{typeof(TAsset).Name}} with name {{name}} was found inside bundle {{foundInBundle}}, it is recommended that you load the asset directly.\");");
            writer.EndBlock();

            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("return asset;");
            writer.EndBlock();

            writer.WriteLine("asset = _assetBundles[bundle].LoadAsset<TAsset>(name);");
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine("if(!asset)");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Warning($\"The method \\\"{{GetCallingMethod()}}\\\" is calling \\\"LoadAsset<TAsset>(string, {_output.bundleEnumName})\\\" with the arguments \\\"{{typeof(TAsset).Name}}\\\", \\\"{{name}}\\\" and \\\"{{bundle}}\\\", however, the asset could not be found.\\n A complete search of all the bundles will be done and the correct bundle enum will be logged.\");");
            writer.WriteLine($"return LoadAsset<TAsset>(name, {_output.bundleEnumName}.All);");
            writer.EndBlock();

            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("return asset;");
            writer.WriteLine();
            WriteFindAssetMethod();
            writer.EndBlock();

            void WriteFindAssetMethod()
            {
                writer.WriteLine($"TAsset FindAsset(string assetName, out {_output.bundleEnumName} foundInBundle)");
                writer.BeginBlock();

                writer.WriteLine("foreach((var enumVal, var assetBundle) in _assetBundles)");
                writer.BeginBlock();

                writer.WriteLine("var loadedAsset = assetBundle.LoadAsset<TAsset>(assetName);");
                writer.WriteLine("if(loadedAsset)");
                writer.BeginBlock();

                writer.WriteLine("foundInBundle = enumVal;");
                writer.WriteLine("return loadedAsset;");
                writer.EndBlock();
                writer.EndBlock();

                writer.WriteLine($"foundInBundle = {_output.bundleEnumName}.Invalid;");
                writer.WriteLine("return null;");
                writer.EndBlock();
            }
        }

        private void WriteLoadAllAssetsMethod()
        {
            writer.WriteLine($"public static TAsset[] LoadAllAssets<TAsset>({_output.bundleEnumName} bundle) where TAsset : UnityEngine.Object");
            writer.BeginBlock();

            writer.WriteLine("List<TAsset> loadedAssets = new List<TAsset>();");
            writer.WriteLine($"if(bundle == {_output.bundleEnumName}.All)");
            writer.BeginBlock();

            writer.WriteLine("FindAssets(loadedAssets);");
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine("if(loadedAssets.Count == 0)");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Warning($\"Could not find any asset of type {{typeof(TAsset).Name}} in any of the bundles.\");");
            writer.EndBlock();

            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("return loadedAssets.ToArray();");
            writer.EndBlock();

            writer.WriteLine("loadedAssets = _assetBundles[bundle].LoadAllAssets<TAsset>().ToList();");
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine("if(loadedAssets.Count == 0)");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Warning($\"Could not find any asset of type {{typeof(TAsset)}} inside the bundle {{bundle}}\");");
            writer.EndBlock();
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("return loadedAssets.ToArray();");
            writer.WriteLine();

            WriteFindAssetsMethod();
            writer.EndBlock();

            void WriteFindAssetsMethod()
            {
                writer.WriteLine("void FindAssets(List<TAsset> output)");
                writer.BeginBlock();

                writer.WriteLine("foreach((var _, var bndle) in _assetBundles)");
                writer.BeginBlock();

                writer.WriteLine("output.AddRange(bndle.LoadAllAssets<TAsset>());");
                writer.EndBlock();
                writer.WriteLine("return;");
                writer.EndBlock();
            }
        }

        private void WriteInitializeMethod()
        {
            writer.WriteLine("internal static IEnumerator Initialize(LoadStaticContentAsyncArgs args)");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Info(\"Initializing Assets\");");
            writer.WriteLine();
            writer.WriteLine("yield return LoadAssetBundles(args);");
            writer.WriteLine("yield return SwapShaders(args);");
            writer.WriteLine("yield return SwapAddressableShaders(args);");
            writer.EndBlock();
        }

        private void WriteLoadAssetBundlesMethod()
        {
            writer.WriteLine("private static IEnumerator LoadAssetBundles(LoadStaticContentAsyncArgs args)");
            writer.BeginBlock();

            writer.WriteLine("var bundlePaths = GetAssetBundlePaths();");
            writer.WriteLine("for(int i = 0; i < bundlePaths.Length; i++)");
            writer.BeginBlock();

            writer.WriteLine("//In this switch statement we proceed to tie a bundle name to a specific enum. there's a special case for default if the loaded bundle is a streamed scene bundle.");
            writer.WriteLine("string path = bundlePaths[i];");
            writer.WriteLine("switch(path)");
            writer.BeginBlock();

            writer.WriteLine($"case MAIN: yield return LoadAndAssign(path, {_output.bundleEnumName}.Main, i); break;");
            writer.WriteLine("default: yield return HandleDefaultCase(path, i); break;");
            writer.EndBlock();
            writer.EndBlock();

            writer.WriteLine();
            WriteLoadAndAssignMethod();
            writer.WriteLine();
            WriteHandleDefaultCaseMethod();
            writer.EndBlock();

            void WriteLoadAndAssignMethod()
            {
                writer.WriteLine($"IEnumerator LoadAndAssign(string path, {_output.bundleEnumName} bundleEnum, int index)");
                writer.BeginBlock();

                writer.WriteLine("var request = AssetBundle.LoadFromFileAsync(path);");
                writer.WriteLine("while(!request.isDone)");
                writer.BeginBlock();
                writer.WriteLine("args.ReportProgress(Util.Remap(request.progress + index, 0f, bundlePaths.Length, 0f, 0.4f));");
                writer.WriteLine("yield return null;");
                writer.EndBlock();
                writer.WriteLine();
                writer.WriteLine("try");
                writer.BeginBlock();

                writer.WriteLine("AssetBundle bundle = request.assetBundle;");
                writer.WriteLine("if(!bundle)");
                writer.BeginBlock();

                writer.WriteLine("throw new FileLoadException(\"AssetBundle.LoadFromFile did not return an asset bundle.\");");
                writer.EndBlock();

                writer.WriteLine("if(_assetBundles.ContainsKey(bundleEnum))");
                writer.BeginBlock();

                writer.WriteLine("throw new InvalidOperationException($\"AssetBundle in path loaded succesfully, but the assetbundles dictionary already contains an entry for {bundleEnum}\");");
                writer.EndBlock();
                writer.WriteLine();
                writer.WriteLine("_assetBundles[bundleEnum] = bundle;");
                writer.EndBlock();
                writer.WriteLine("catch(Exception e)");
                writer.BeginBlock();

                writer.WriteLine($"{data.loggerClassName}.Error($\"Could not load assetbundle at path {{path}} andd assign to enum {{bundleEnum}}. {{e}}\");");
                writer.EndBlock();
                writer.EndBlock();
            }

            void WriteHandleDefaultCaseMethod()
            {
                writer.WriteLine($"IEnumerator HandleDefaultCase(string path, int index)");
                writer.BeginBlock();

                writer.WriteLine("var request = AssetBundle.LoadFromFileAsync(path);");
                writer.WriteLine("while(!request.isDone)");
                writer.BeginBlock();
                writer.WriteLine("args.ReportProgress(Util.Remap(request.progress + index, 0f, bundlePaths.Length, 0f, 0.4f));");
                writer.WriteLine("yield return null;");
                writer.EndBlock();
                writer.WriteLine();
                writer.WriteLine("try");
                writer.BeginBlock();

                writer.WriteLine("AssetBundle bundle = request.assetBundle;");
                writer.WriteLine("if(!bundle)");
                writer.BeginBlock();

                writer.WriteLine("throw new FileLoadException(\"AssetBundle.LoadFromFile did not return an asset bundle. (Path={path})\");");
                writer.EndBlock();

                writer.WriteLine("if(!bundle.isStreamedSceneAssetBundle)");
                writer.BeginBlock();

                writer.WriteLine("throw new Exception($\"AssetBundle in specified path is not a streamed scene bundle, but its file name was not found in the Switch statement. have you forgotten to setup the enum and file name in your assets class? (Path={path})\");");
                writer.EndBlock();
                writer.WriteLine("else");
                writer.BeginBlock();

                writer.WriteLine("HG.ArrayUtils.ArrayAppend(ref _streamedSceneBundles, bundle);");
                writer.EndBlock();
                writer.WriteLine($"{data.loggerClassName}.Warning($\"Invalid or Unexpected file in the AssetBundles folder (Path={{path}})\");");
                writer.EndBlock();
                writer.WriteLine("catch(Exception e)");
                writer.BeginBlock();

                writer.WriteLine($"{data.loggerClassName}.Error($\"Default statement on bundle loading method hit, Exception thrown. {{e}}\");");
                writer.EndBlock();
                writer.EndBlock();
            }
        }

        private void WriteSwapShaders()
        {
            writer.WriteLine("private static IEnumerator SwapShaders(LoadStaticContentAsyncArgs args)");
            writer.BeginBlock();

            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine($"{data.loggerClassName}.Debug(\"Swapping stubbed shaders from AssetBundles\");");
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine($"var request = new MultipleBundleAssetRequest<Material>({_output.bundleEnumName}.All);");
            writer.WriteLine("yield return LoadAllAssetsAsync(request);");
            writer.WriteLine();
            writer.WriteLine("var materials = request.Assets.Where(mat => mat.shader.name.StartsWith(\"Stubbed\")).ToArray();");
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

            writer.WriteLine($"{data.loggerClassName}.Error($\"Faileed to swap the shader of material \\\"{{material}}\\\". {{e}}\");");
            writer.WriteLine("continue;");
            writer.EndBlock();
            writer.WriteLine("while(!asyncOp.IsDone)");
            writer.BeginBlock();

            writer.WriteLine("args.ReportProgress(Util.Remap(asyncOp.PercentComplete + i, 0f, 1f, 0.4f, 0.6f));");
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
            writer.WriteLine($"{data.loggerClassName}.Debug(\"Finalizing materials with AddressableMaterial shaders from AssetBundles\");");
            writer.WritePreprocessorDirectiveLine("endif");

            writer.WriteLine($"var request = new MultipleBundleAssetRequest<Material>({_output.bundleEnumName}.All);");
            writer.WriteLine("yield return LoadAllAssetsAsync(request);");
            writer.WriteLine();
            writer.WriteLine("var materials = request.Assets.Where(mat => mat.shader.name == \"AddressableMaterialShader\").ToArray();");
            writer.WriteLine("for(int i = 0; i < materials.Length; i++)");
            writer.BeginBlock();

            writer.WriteLine("var material = materials[i];");
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine($"{data.loggerClassName}.Debug($\"Finalizing material {{material}}\");");
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine();
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

        private void WriteGetCallingMethod()
        {
            writer.WriteLine("private static string GetCallingMethod()");
            writer.BeginBlock();

            writer.WriteLine("var stackTrace = new StackTrace();");
            writer.WriteLine();
            writer.WriteLine("for(int stackFrameIndex = 0; stackFrameIndex < stackTrace.FrameCount; stackFrameIndex++)");
            writer.BeginBlock();

            writer.WriteLine("var frame = stackTrace.GetFrame(stackFrameIndex);");
            writer.WriteLine("var method = frame.GetMethod();");
            writer.WriteLine("if(method == null)");
            writer.WriteLine("continue;");
            writer.WriteLine();
            writer.WriteLine("var declaringType = method.DeclaringType;");
            writer.WriteLine($"if(declaringType.IsGenericType && declaringType.DeclaringType == typeof({_output.assetBundleClassName}))");
            writer.WriteLine("continue;");
            writer.WriteLine();
            writer.WriteLine($"if(declaringType == typeof({_output.assetBundleClassName}))");
            writer.WriteLine("continue;");
            writer.WriteLine();
            writer.WriteLine("var fileName = frame.GetFileName();");
            writer.WriteLine("var fileLineNumber = frame.GetFileLineNumber();");
            writer.WriteLine("var fileColumnNumber = frame.GetFileColumnNumber();");
            writer.WriteLine();
            writer.WriteLine("return $\"{declaringType.FullName}.{method.Name}({GetMethodParams(method)}) (fileName={fileName}, Location=L{fileLineNumber} C{fileColumnNumber})\";");
            writer.EndBlock();

            writer.WriteLine("return \"[COULD NOT GET CALLING METHOD]\";");
            writer.EndBlock();
        }

        private void WriteGetMethodParams()
        {
            writer.WriteLine("private static string GetMethodParams(MethodBase methodBase)");
            writer.BeginBlock();

            writer.WriteLine("var parameters = methodBase.GetParameters();");
            writer.WriteLine("if(parameters.Length == 0)");
            writer.WriteLine("return string.Empty;");
            writer.WriteLine();
            writer.WriteLine("StringBuilder stringBuilder = new StringBuilder();");
            writer.WriteLine("foreach(var parameter in parameters)");
            writer.BeginBlock();

            writer.WriteLine("stringBuilder.Append(parameter.ToString() + \", \");");
            writer.EndBlock();

            writer.WriteLine("return stringBuilder.ToString();");
            writer.EndBlock();
        }

        private void WriteSingleBundleAssetRequest()
        {
            writer.WriteLine("public class SingleBundleAssetRequest<TAsset> where TAsset : UnityEngine.Object");
            writer.BeginBlock();

            writer.WriteLine("public TAsset Asset { get; private set; }");
            writer.WriteLine($"public {_output.bundleEnumName} BundleEnum {{ get; private set; }}");
            writer.WriteLine("private string assetName;");
            writer.WriteLine();
            writer.WriteLine("internal IEnumerator Load()");
            writer.BeginBlock();

            writer.WriteLine($"if(BundleEnum == {_output.bundleEnumName}.All)");
            writer.BeginBlock();

            writer.WriteLine("yield return FindAsset();");
            writer.WriteLine();
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine("if(!Asset)");
            writer.BeginBlock();

            writer.WriteLine($"BundleEnum = {_output.bundleEnumName}.Invalid;");
            writer.WriteLine($"{data.loggerClassName}.Warning($\"Could not find asset of type {{typeof(TAsset).Name}} with name {{assetName}} in any of the bundles.\");");
            writer.EndBlock();
            writer.WriteLine("else");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Info($\"Asset of type {{typeof(TAsset).Name}} with name {{assetName}} was found inside bundle {{BundleEnum}}, it is recommended that you load the asset directly.\");");
            writer.EndBlock();
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("yield break;");
            writer.EndBlock();
            writer.WriteLine();
            writer.WriteLine("var request = _assetBundles[BundleEnum].LoadAssetAsync<TAsset>(assetName);");
            writer.WriteLine("while(!request.isDone)");
            writer.BeginBlock();

            writer.WriteLine("yield return null;");
            writer.EndBlock();

            writer.WriteLine();
            writer.WriteLine("Asset = (TAsset)request.asset;");
            writer.WriteLine();
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine("if(!Asset)");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Warning($\"The method \\\"{{GetCallingMethod()}}\\\" is calling \\\"LoadAssetAsync<TAsset>(SingleBundleAssetRequest<TAsset>)\\\" with the arguments \\\"{{typeof(TAsset).Name}}\\\", \\\"{{assetName}}\\\" and \\\"{{BundleEnum}}\\\", however, the asset could not be found.\\n A complete search of all the bundles will be done and the correct bundle enum will be logged.\");");
            writer.EndBlock();
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine("yield break;");
            writer.EndBlock();
            writer.WriteLine();
            WriteFindAsset();
            writer.WriteLine();
            writer.WriteLine($"public SingleBundleAssetRequest(string name, {_output.bundleEnumName} bundleEnum)");
            writer.BeginBlock();

            writer.WriteLine("assetName = name;");
            writer.WriteLine("BundleEnum = bundleEnum;");
            writer.EndBlock();
            writer.EndBlock();

            void WriteFindAsset()
            {
                writer.WriteLine("private IEnumerator FindAsset()");
                writer.BeginBlock();

                writer.WriteLine("foreach((var enumVal, var assetBundle) in _assetBundles)");
                writer.BeginBlock();

                writer.WriteLine("var request = assetBundle.LoadAssetAsync<TAsset>(assetName);");
                writer.WriteLine("while(request.isDone)");
                writer.BeginBlock();

                writer.WriteLine("yield return null;");
                writer.EndBlock();

                writer.WriteLine("Asset = (TAsset)request.asset;");
                writer.WriteLine("if(Asset)");
                writer.BeginBlock();
                writer.WriteLine("BundleEnum = enumVal;");
                writer.WriteLine("yield break;");
                writer.EndBlock();
                writer.EndBlock();
                writer.EndBlock();
            }
        }

        private void WriteMultipleBundleAssetRequest()
        {
            writer.WriteLine("public class MultipleBundleAssetRequest<TAsset> where TAsset : UnityEngine.Object");
            writer.BeginBlock();

            writer.WriteLine("public TAsset[] Assets { get; private set; }");
            writer.WriteLine($"public {_output.bundleEnumName} BundleEnum {{ get; private set; }}");
            writer.WriteLine();

            writer.WriteLine("internal IEnumerator Load()");
            writer.BeginBlock();

            writer.WriteLine("List<TAsset> assets = new List<TAsset>();");
            writer.WriteLine($"if(BundleEnum == {_output.bundleEnumName}.All)");
            writer.BeginBlock();

            writer.WriteLine("yield return FindAssets(assets);");
            writer.WriteLine();
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine("if(assets.Count == 0)");

            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Warning($\"Could not find any asset of type {{typeof(TAsset).Name}} in any of the bundles.\");");
            writer.EndBlock();
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine();
            writer.WriteLine("Assets = assets.ToArray();");
            writer.WriteLine("yield break;");
            writer.EndBlock();

            writer.WriteLine();
            writer.WriteLine("var request = _assetBundles[BundleEnum].LoadAllAssetsAsync<TAsset>();");
            writer.WriteLine("while(!request.isDone)");
            writer.BeginBlock();

            writer.WriteLine("yield return null;");
            writer.EndBlock();

            writer.WriteLine("assets.AddRange(request.allAssets.OfType<TAsset>());");
            writer.WriteLine("Assets = assets.ToArray();");
            writer.WriteLine();
            writer.WritePreprocessorDirectiveLine("if DEBUG");
            writer.WriteLine("if(Assets.Length == 0)");
            writer.BeginBlock();

            writer.WriteLine($"{data.loggerClassName}.Warning($\"Could not find any asset of type {{typeof(TAsset)}} inside the bundle {{BundleEnum}}\");");
            writer.EndBlock();
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine();
            writer.WriteLine("yield break;");
            writer.EndBlock();
            writer.WriteLine();
            WriteFindAssets();
            writer.WriteLine();
            writer.WriteLine($"public MultipleBundleAssetRequest({_output.bundleEnumName} bundleEnum)");
            writer.BeginBlock();

            writer.WriteLine("BundleEnum = bundleEnum;");
            writer.EndBlock();
            writer.EndBlock();

            void WriteFindAssets()
            {
                writer.WriteLine("private IEnumerator FindAssets(List<TAsset> output)");
                writer.BeginBlock();

                writer.WriteLine("foreach((var _, var bundle) in _assetBundles)");
                writer.BeginBlock();

                writer.WriteLine("var request = bundle.LoadAllAssetsAsync<TAsset>();");
                writer.WriteLine("while(!request.isDone)");
                writer.BeginBlock();

                writer.WriteLine("yield return null;");
                writer.EndBlock();

                writer.WriteLine("output.AddRange(request.allAssets.OfType<TAsset>());");
                writer.EndBlock();

                writer.WriteLine("yield break;");
                writer.EndBlock();
            }
        }

        public MultipleAssetBundleAssetsClassCodeGen(Data data)
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
            public string assetBundleName;

            public string path;
        }

        public struct Output
        {
            public string assetBundleClassName;
            public string bundleEnumName;
        }
    }
}
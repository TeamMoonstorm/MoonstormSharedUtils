using RoR2EditorKit;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;
using Moonstorm.Editor.CodeGen;
using RoR2EditorKit.CodeGen;

namespace Moonstorm.Editor.Windows
{
    public class MSUModCreatorWindow : RoR2EditorKit.EditorWindows.CreatorWizardWindow
    {

        public string authorName = "EnterYourNameHere";
        public string modName = "YourMod";
        public string humanReadableModName = "Your Mod";
        public string modDescription = "Lorem Ipsum...";
        public AssetClassType assetsClassType = AssetClassType.SingleBundle;
        protected override string WizardTitleTooltip =>
@"The ModCreatorWizard is a custom wizard that creates the following upon completion:
1.- An AssemblyDef with references to most common ror2 modding assemblies, alongside MSU and Risk of Options
2.- A very basic MainClass following a basic Singleton pattern
3.- An Assets class, which can manage a single or multiple assetbundle format
4.- A Content class, which handles ContentPack loading and addition
5.- A Language class, which will load your language files.
6.- A Config class, which can be used to create new Configs and working with MSU's Config Systems.
7.- A Logger class, which has utilities related to logging and debugging.
8.- A folder for your Assets for the AssetBundle
9.- A ThunderKit Manifest for your mod";

        private List<Assembly> _assemblyList = new List<Assembly>();
        private List<string> _assemblyNames = new List<string>();

        private string _directory;
        private string _loggerClassName;
        private string _mainClassName;
        private string _assetsClassName;
        private string _bundleEnumName;

        private string _contentClassName;
        [MenuItem("Assets/Create/MSUMod", priority = ThunderKit.Common.Constants.ThunderKitMenuPriority)]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<MSUModCreatorWindow>();
            window.Focus();
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            _assemblyList = AppDomain.CurrentDomain.GetAssemblies().ToList();
            _assemblyNames = _assemblyList.Select(asm => asm.GetName().Name).ToList();

            var textField = WizardElementContainer.Q<TextField>(nameof(authorName));
            textField.RegisterValueChangedCallback(s =>
            {
                authorName = s.newValue.Replace(' ', '_');
            });
            textField.isDelayed = true;

            textField = WizardElementContainer.Q<TextField>(nameof(modName));
            textField.RegisterValueChangedCallback(s =>
            {
                modName = s.newValue.Replace(' ', '_');
            });
            textField.isDelayed = true;
        }
        protected override bool ValidateUXMLPath(string path)
        {
            return path.Contains("teammoonstorm-moonstormsharedutils") || path.Contains("riskofthunder-ror2editorkit");
        }
        protected override async Task<bool> RunWizard()
        {
            _directory = string.Empty;
            _loggerClassName = string.Empty;
            _mainClassName = string.Empty;
            _assetsClassName = string.Empty;
            _bundleEnumName = string.Empty;
            if(authorName.IsNullOrEmptyOrWhitespace() || modName.IsNullOrEmptyOrWhitespace())
            {
                Debug.LogError("authorName or modName is null, empty, or whitespace");
                return false;
            }

            try
            {
                await CreateFolder();
                await CreateAssemblyDef();
                await CreateLoggerClass();
                await CreateMainClass();
                await CreateAssetsClass();
                //await CreateContentClass();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            return true;
        }

        private Task CreateFolder()
        {
            _directory = IOUtils.GetCurrentDirectory();
            _directory += $"/{modName}";
            _directory = IOUtils.FormatPathForUnity(_directory);
            IOUtils.EnsureDirectory(_directory);

            AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(_directory));
            return Task.CompletedTask;
        }

        private async Task CreateAssemblyDef()
        {
            var def = new ThunderKit.Core.Data.AssemblyDef();
            def.name = modName;
            def.references = new string[]
            {
                "Unity.Postprocessing.Runtime",
                "com.unity.multiplayer-hlapi.Runtime",
                "Unity.TextMeshPro",
                "UnityEngine.UI",
                "MSU.Runtime",
                "Wwise"
            };
            def.overrideReferences = true;

            var precompiledReferencesList = new List<string>()
            {
                "BepInEx.dll",
                "R2API.dll",
                "MonoMod.Utils.dll",
                "Mono.Cecil.dll",
                "MMHOOK_RoR2.dll",
                "HGCSharpUtils.dll",
                "HGUnityUtils.dll",
                "Zio.dll",
                "RoR2.dll",
                "RoR2BepInExPack.dll",
                "Unity.Addressables.dll",
                "Unity.ResourceManager.dll",
                "Unity.TextMeshPro.dll",
                "UnityEngine.UI.dll",
                "Unity.Postprocessing.Runtime.dll",
                "RiskOfOptions.dll",
                "Wwise.dll"
            };
            precompiledReferencesList.AddRange(_assemblyNames.Where(asm => asm.StartsWith("R2API.")).Select(asm => $"{asm}.dll"));

            def.precompiledReferences = precompiledReferencesList.ToArray();
            def.autoReferenced = true;

            string assemblyDefPath = Path.Combine(_directory, $"{modName}.asmdef");
            using(var fs = File.CreateText(assemblyDefPath))
            {
                await fs.WriteAsync(EditorJsonUtility.ToJson(def, true));
            }
            var projectRelativePath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(assemblyDefPath));
            AssetDatabase.ImportAsset(projectRelativePath, ImportAssetOptions.Default);
        }

        private async Task CreateLoggerClass()
        {
            var loggerClassPath = Path.Combine(_directory, $"{modName}Logger.cs");
            var codeGen = new LoggerClassCodeGen(new LoggerClassCodeGen.Data
            {
                modName = modName,
                path = loggerClassPath,
            });

            var writer = codeGen.WriteCode(out var output);
            _loggerClassName = output.loggerClassName;

            var validationData = new CodeGeneratorValidator.ValidationData
            {
                code = writer,
                desiredPath = loggerClassPath
            };

            await CodeGeneratorValidator.ValidateAsync(validationData);
        }

        private async Task CreateMainClass()
        {
            var mainClassPath = Path.Combine(_directory, $"{modName}Main.cs");
            var codeGen = new MainClassCodeGen(new MainClassCodeGen.Data
            {
                authorName = authorName,
                humanReadableModName = humanReadableModName,
                modName = modName,
                loggerClassName = _loggerClassName,
                path = mainClassPath,
            });

            var writer = codeGen.WriteCode(out var output);
            _mainClassName = output.mainClassName;

            var validationData = new CodeGeneratorValidator.ValidationData
            {
                code = writer,
                desiredPath = mainClassPath
            };
            await CodeGeneratorValidator.ValidateAsync(validationData);
        }

        private async Task CreateAssetsClass()
        {
            var assetsClassPath = Path.Combine(_directory, $"{modName}Assets.cs");
            Writer writer = default;
            switch(assetsClassType)
            {
                case AssetClassType.SingleBundle:
                {
                    var codeGen = new SingleAssetBundleAssetsClassCodeGen(new SingleAssetBundleAssetsClassCodeGen.Data
                    {
                        assetbundleName = modName.ToLowerInvariant() + "assets",
                        loggerClassName = _loggerClassName,
                        mainClassName = _mainClassName,
                        modName = modName,
                        path = assetsClassPath,
                    });
                    writer = codeGen.WriteCode(out var output);
                    _assetsClassName = output.assetbundleClassName;
                    break;
                }
                case AssetClassType.MultipleBundle:
                {
                    var codeGen = new MultipleAssetBundleAssetsClassCodeGen(new MultipleAssetBundleAssetsClassCodeGen.Data
                    {
                        assetBundleName = modName.ToLowerInvariant() + "main",
                        loggerClassName = _loggerClassName,
                        mainClassName = _mainClassName,
                        modName = modName,
                        path = assetsClassPath
                    });
                    writer = codeGen.WriteCode(out var output);
                    _assetsClassName = output.assetBundleClassName;
                    _bundleEnumName = output.bundleEnumName;
                    break;
                }
            }

            var validationData = new CodeGeneratorValidator.ValidationData
            {
                code = writer,
                desiredPath = assetsClassPath
            };
            await CodeGeneratorValidator.ValidateAsync(validationData);
        }

        private async Task CreateContentClass()
        {
            var contentClassPath = Path.Combine(_directory, $"{modName}Content.cs");
            Writer writer = default;

            var codeGen = new ContentClassCodeGen(new ContentClassCodeGen.Data
            {
                assetbundleClassName = _assetsClassName,
                isSingleAssetsClass = assetsClassType == AssetClassType.SingleBundle,
                loggerClassName = _loggerClassName,
                mainClassName = _mainClassName,
                modName = modName,
                path = contentClassPath
            });
            writer = codeGen.WriteCode(out var output);
            _contentClassName = output.contentClassName;

            var validationDdata = new CodeGeneratorValidator.ValidationData
            {
                code = writer,
                desiredPath = contentClassPath
            };
            await CodeGeneratorValidator.ValidateAsync(validationDdata);
        }
        public enum AssetClassType
        {
            SingleBundle,
            MultipleBundle
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThunderKit.Integrations.Thunderstore;
using ThunderKit.Core.Config;
using UnityEngine.UIElements;
using UnityEditor;
using ThunderKit.Core.Utilities;
using System;
using ThunderKit.Core.Data;
using System.Linq;

namespace Moonstorm.EditorUtils.Importers
{
    public class InstallMSUDependencies : OptionalExecutor
    {
        public override int Priority => Constants.Priority.InstallDependecies;
        public override string Description => $"Installs the following soft dependecies from MSU:\nDebugToolkit: Used for Debugging purposes\nRisk Of Options: Allows compatibility with ConfigurableField attribute.";
        public override string Name => $"Install MSU Dependencies";

        public bool installDebugToolkit;
        public bool installRiskOfOptions;

        private ThunderstoreSource transientStore;
        private string ThunderstoreAddress => "https://thunderstore.io";
        private const string transientStoreName = "transient-store";
        protected override VisualElement CreateProperties()
        {
            VisualElement root = new VisualElement();
            root.name = "InstallMSUDependecies_Root";

            Toggle debugToolkitToggle = new Toggle("Install DebugToolkit");
            debugToolkitToggle.tooltip = "DebugToolkit: Used for Debugging purposes";
            debugToolkitToggle.bindingPath = "installDebugToolkit";
            root.Add(debugToolkitToggle);

            Toggle riskOfOptionsToggle = new Toggle("Install RiskOfOptions");
            riskOfOptionsToggle.tooltip = "Risk Of Options: Allows compatibility with ConfigurableField attribute";
            riskOfOptionsToggle.bindingPath = "installRiskOfOptions";
            root.Add(riskOfOptionsToggle);

            return root;
        }
        public sealed override bool Execute()
        {
            try
            {
                EditorApplication.LockReloadAssemblies();
                if(installDebugToolkit)
                {
                    InstallPackageSingle("IHarbHD-DebugToolkit");
                }
                if(installRiskOfOptions)
                {
                    InstallPackageSingle("Rune580-Risk_Of_Options");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return false;
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }

            PackageHelper.ResolvePackages();

            return true;
        }

        public override void Cleanup()
        {
            if (transientStore)
                DestroyImmediate(transientStore);
        }

        private void InstallPackageSingle(string id)
        {
            var packageSource = PackageSourceSettings.PackageSources.OfType<ThunderstoreSource>().FirstOrDefault(source => source.Url == ThunderstoreAddress);
            if (!packageSource)
            {
                if (transientStore)
                    packageSource = transientStore;
                else
                {
                    packageSource = CreateInstance<ThunderstoreSource>();
                    packageSource.Url = ThunderstoreAddress;
                    packageSource.name = transientStoreName;
                    packageSource.ReloadPages();
                    transientStore = packageSource;
                }
            }
            else if (packageSource.Packages == null || packageSource.Packages.Count == 0)
            {
                packageSource.ReloadPages();
            }

            if (packageSource.Packages == null || packageSource.Packages.Count == 0)
            {
                Debug.LogWarning($"PackageSource at \"{ThunderstoreAddress}\" has no packages");
            }

            var package = packageSource.Packages.FirstOrDefault(pkg => pkg.DependencyId == id);
            if (package == null)
            {
                Debug.LogWarning($"Could not find package with DependencyId of \"{id}\"");
            }

            if (package.Installed)
            {
                Debug.LogWarning($"Not installing package with DependencyId of \"{id}\" because it's already installed");
            }

            Debug.Log($"Installing latest version of package \"{id}\"");
            var task = packageSource.InstallPackage(package, "latest");
            while (!task.IsCompleted)
            {
                Debug.Log("Waiting for completion");
            }
        }
    }
}

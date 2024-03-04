using UnityEngine;
using System.Linq;
using ThunderKit.Core.Config;
using ThunderKit.Core.Data;
using ThunderKit.Integrations.Thunderstore;
using UnityEditor;
using Boo.Lang;
using ThunderKit.Core.Windows;
using ThunderKit.Core.Utilities;

namespace MSU.Editor.Importers
{
    public class InstallDependencies : OptionalExecutor
    {
        public override int Priority => Constants.Priority.InstallDependencies;

        public override string Description => "Installs Risk of Options, a framework that allows MSU to display your mod's configs in the game's options menu.";

        private const string TRANSIENT_STORE_NAME = "transient-store";
        private const string RISK_OF_OPTIONS = "Rune580-Risk_Of_Options";
        private const string SHADER_SWAPPER = "Smooth_Salad-ShaderSwapper";

        private ThunderstoreSource transientStore;
        public override bool Execute()
        {
            try
            {
                EditorApplication.LockReloadAssemblies();
                var packageSource = PackageSourceSettings.PackageSources.OfType<ThunderstoreSource>().FirstOrDefault(source => source.Url == "https://thunderstore.io");

                if (!packageSource)
                {
                    if (transientStore)
                        packageSource = transientStore;
                    else
                    {
                        packageSource = CreateInstance<ThunderstoreSource>();
                        packageSource.Url = "https://thunderstore.io";
                        packageSource.name = TRANSIENT_STORE_NAME;
                        packageSource.ReloadPages(true);
                        transientStore = packageSource;
                    }
                }
                else if (packageSource.Packages == null || packageSource.Packages.Count == 0)
                    packageSource.ReloadPages(true);
                else
                    transientStore = packageSource;

                if(packageSource.Packages == null || packageSource.Packages.Count == 0)
                {
                    Debug.LogWarning($"PackageSource at \"{packageSource.Url}\" has no packages");
                    return false;
                }

                List<(PackageGroup, string)> packages = new List<(PackageGroup, string)>();
                foreach(string dependency in GetDependencyIDs())
                {
                    var pkg = packageSource.Packages.FirstOrDefault(p => p.DependencyId == dependency);

                    if(pkg != null && !pkg.Installed)
                    {
                        packages.Add((pkg, "latest"));
                    }
                }

                if (packages.Count == 0)
                    return true;

                var task = packageSource.InstallPackages(packages);
                while(!task.IsCompleted)
                {
                    Debug.Log("Waiting for Completion...");
                }
            }
            catch(System.Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }

            PackageHelper.ResolvePackages();
            return true;
        }

        private List<string> GetDependencyIDs()
        {
            return new List<string>
            {
                RISK_OF_OPTIONS,
                SHADER_SWAPPER
            };
        }

        public override void Cleanup()
        {
            if (transientStore)
                DestroyImmediate(transientStore);
        }
    }
}

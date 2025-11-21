using RoR2;
using RoR2.Editor;
using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;
using Path = System.IO.Path;

#pragma warning disable CS0618 // Type or member is obsolete
namespace MSU.Editor.EditorWindows
{
    public class SkinDefMigrationWizard : EditorWizardWindow
    {
        public SkinDef[] skinsToMigrate = Array.Empty<SkinDef>();
        public VanillaSkinDef[] vanillaSkinDefsToMigrate = Array.Empty<VanillaSkinDef>();

        protected override string wizardTitleTooltip => 
@"The SkinDefMigrationWizard is a Wizard that migrates both base game SkinDefs and MSU VanillaSkinDefs into the new system called UberSkinDef.

It'll create a new ScriptableObject utilized by MSU called an UberSkinDef, where the serialized data will be stored instead of in the SkinDef or the SkinDefParams.

If the skin being migrated lacks a SkinDefParams, a new one will be created. Skins will follow the following naming convention
Uber_ASSETNAME.asset == UberSkinDef
ASSETNAME.asset == SkinDef
ASSETNAME_params.asset = SkinDefParams";

        [MenuItem(MSUConstants.MSU_MENU_ROOT + "Wizards/SkinDefMigrationWizard")]
        private static void Open() => Open<SkinDefMigrationWizard>(null);
        protected override IEnumerator RunWizardCoroutine()
        {
            yield break;
        }

        protected override void SetupControls()
        {
            Button loadAllSkinTypes = rootVisualElement.Q<Button>("LoadAllSkinTypes");

            loadAllSkinTypes.clicked += LoadAllSkinTypes;
        }

        private void LoadAllSkinTypes()
        {
            var skinDefs = AssetDatabaseUtil.FindAssetsByType<SkinDef>().ToList();
            var vanillaSkinDefs = AssetDatabaseUtil.FindAssetsByType<VanillaSkinDef>().ToList();

            for(int i = skinDefs.Count - 1; i >= 0; i--)
            {
                var skinDef = skinDefs[i];
                //VanillaSkinDefs are treated differently
                if(vanillaSkinDefs.Contains(skinDef))
                {
                    skinDefs.RemoveAt(i);
                    continue;
                }

                if(IsSkinMigrated(skinDef))
                {
                    skinDefs.RemoveAt(i);
                    continue;
                }
            }

            skinsToMigrate = skinDefs.ToArray();

            for(int i = vanillaSkinDefs.Count - 1; i >= 0; i--)
            {
                var vanillaSkinDef = vanillaSkinDefs[i];

                if(IsSkinMigrated(vanillaSkinDef))
                {
                    vanillaSkinDefs.RemoveAt(i);
                    continue;
                }
            }

            vanillaSkinDefsToMigrate = vanillaSkinDefs.ToArray();
        }

        private bool IsSkinMigrated(SkinDef skinDef)
        {
            //if there are no params then its definetly not migrated.
            if (skinDef.skinDefParams == null)
                return false;

            //We'll search within the directory to check if there's an UberSkinDef that references this SkinDef
            string assetPath = AssetDatabase.GetAssetPath(skinDef);
            string fullPath = Path.GetFullPath(assetPath);
            string directoryName = Path.GetDirectoryName(fullPath);

            string[] files = Directory.GetFiles(directoryName, "*.asset", SearchOption.TopDirectoryOnly);

            for(int i = 0; i < files.Length; i++)
            {
                var filePath = files[i];
                var projectRelative = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(filePath));

                if(projectRelative.IsNullOrEmptyOrWhiteSpace())
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

                if(asset is not UberSkinDef uberSkinDef)
                {
                    continue;
                }

                //This uber skin def targets the skindef we're checking, return true.
                if(uberSkinDef.targetSkinDef && uberSkinDef.targetSkinDef == skinDef)
                {
                    return true;
                }
            }

            return false;
        }

        protected override bool ValidateUXMLPath(string path)
        {
            return path.ValidateUXMLPath();
        }
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
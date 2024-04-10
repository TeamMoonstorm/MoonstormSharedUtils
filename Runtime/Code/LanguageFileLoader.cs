using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MSU
{
    public static class LanguageFileLoader
    {
        private static List<(BaseUnityPlugin, string)> _pluginsWithLanguageFiles = new List<(BaseUnityPlugin, string)>();
        public static void AddLanguageFilesFromMod(BaseUnityPlugin baseUnityPlugin, string languageFolderName)
        {
            _pluginsWithLanguageFiles.Add((baseUnityPlugin, languageFolderName));
        }

        static LanguageFileLoader()
        {
            On.RoR2.Language.SetFolders += AddLanguageFiles;
        }

        private static void AddLanguageFiles(On.RoR2.Language.orig_SetFolders orig, RoR2.Language self, IEnumerable<string> newFolders)
        {
            List<string> moddedFolders = new List<string>();

            foreach(var (plugin, langFolderName) in _pluginsWithLanguageFiles)
            {
                var directoryName = Path.GetDirectoryName(plugin.Info.Location);
                var lantguageDirectory = Path.Combine(directoryName, langFolderName);

                if(Directory.Exists(lantguageDirectory))
                {
                    var newDirectories = Directory.EnumerateDirectories(lantguageDirectory, self.name);
                    moddedFolders.AddRange(newDirectories);
                }
            }
            moddedFolders.AddRange(newFolders);
            orig(self, moddedFolders);
        }
    }
}

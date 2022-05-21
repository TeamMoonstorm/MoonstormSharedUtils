using RoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace Moonstorm.Loaders
{
    public abstract class LanguageLoader<T> : LanguageLoader where T : LanguageLoader<T>
    {
        public static T Instance { get; private set; }

        public LanguageLoader()
        {
            try
            {
                if (Instance != null)
                {
                    throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting LanguageLoader was instantiated twice");
                }
                Instance = this as T;
            }
            catch (Exception e)
            {
                MSULog.Error(e);
            }
        }
    }

    public abstract class LanguageLoader
    {
        public abstract string AssemblyDir { get; }
        public abstract string LanguagesFolderName { get; }

        protected void LoadLanguages()
        {
            On.RoR2.Language.SetFolders += AddLanguageFile;
        }

        private void AddLanguageFile(On.RoR2.Language.orig_SetFolders orig, Language self, IEnumerable<string> newFolders)
        {
            if(Directory.Exists(Path.Combine(AssemblyDir, LanguagesFolderName)))
            {
                var dirs = Directory.EnumerateDirectories(Path.Combine(AssemblyDir, LanguagesFolderName), self.name);
                orig(self, newFolders.Union(dirs));
                return;
            }
            orig(self, newFolders);
        }
    }
}

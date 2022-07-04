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
    /// <summary>
    /// The LanguageLoader is a class that can be used to load LanguageFiles into the game's <see cref="RoR2.Language"/> systems
    /// <para>Loading language files this way is required for the <see cref="TokenModifierManager"/> and <see cref="TokenModifierAttribute"/> to work properly</para>
    /// <para>LanguageLoader inheriting classes are treated as Singletons</para>
    /// </summary>
    /// <typeparam name="T">The class that's inheriting from LanguageLoader</typeparam>
    public abstract class LanguageLoader<T> : LanguageLoader where T : LanguageLoader<T>
    {
        /// <summary>
        /// Retrieves the instance of <typeparamref name="T"/>
        /// </summary>
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

    /// <summary>
    /// <inheritdoc cref="LanguageLoader{T}"/>
    /// <para>You probably want to use <see cref="LanguageLoader{T}"/> instead</para>
    /// </summary>
    public abstract class LanguageLoader
    {
        /// <summary>
        /// The directory where your assembly is located
        /// </summary>
        public abstract string AssemblyDir { get; }
        /// <summary>
        /// The root folder of your Language tree
        /// </summary>
        public abstract string LanguagesFolderName { get; }

        /// <summary>
        /// Hooks into <see cref="RoR2.Language.SetFolders(IEnumerable{string})"/> and adds the languages files that are inside <see cref="LanguagesFolderName"/>
        /// </summary>
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

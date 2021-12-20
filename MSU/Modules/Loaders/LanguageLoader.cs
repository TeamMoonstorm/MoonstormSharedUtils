using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zio;
using Zio.FileSystems;

namespace Moonstorm.Loaders
{
    /// <summary>
    /// Class for loading Language Files
    /// <para>Automatically adds a Language folder to the game so it loads into the game's token dictionary</para>
    /// </summary>
    /// <typeparam name="T">The instance of your class</typeparam>
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
                MSULog.LogE(e);
            }
        }
    }

    /// <summary>
    /// Class for loading Language Files
    /// <para>Automatically adds a Language folder to the game so it loads into the game's token dictionary</para>
    /// <para>Inherit from LanguageLoaderT instead</para>
    /// </summary>
    public abstract class LanguageLoader
    {
        /// <summary>
        /// The directory of your assembly
        /// </summary>
        public abstract string AssemblyDir { get; }

        /// <summary>
        /// The name of the Folder that contains the language files
        /// </summary>
        public abstract string LanguagesFolderName { get; }

        public FileSystem FileSystem { get; private set; }

        /// <summary>
        /// Loads the LanguageFile into the game
        /// </summary>
        protected void LoadLanguages()
        {
            PhysicalFileSystem physicalFileSystem = new PhysicalFileSystem();

            FileSystem = new SubFileSystem(physicalFileSystem, physicalFileSystem.ConvertPathFromInternal(AssemblyDir), true);

            if(FileSystem.DirectoryExists($"/{LanguagesFolderName}/"))
            {
                Language.collectLanguageRootFolders += (list) =>
                {
                    list.Add(FileSystem.GetDirectoryEntry($"/{LanguagesFolderName}/"));
                };
            }
        }
    }
}

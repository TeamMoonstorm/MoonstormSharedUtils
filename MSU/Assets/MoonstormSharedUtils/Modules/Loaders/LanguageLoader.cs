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

        public FileSystem FileSystem { get; private set; }

        protected void LoadLanguages()
        {
            PhysicalFileSystem physicalFileSystem = new PhysicalFileSystem();

            FileSystem = new SubFileSystem(physicalFileSystem, physicalFileSystem.ConvertPathFromInternal(AssemblyDir), true);

            if(FileSystem.DirectoryExists($"/{LanguagesFolderName}/"))
            {
                Language.collectLanguageRootFolders += (list) =>
                {
                    //list.Add(FileSystem.GetDirectoryEntry($"/{LanguagesFolderName}/")); //CS1503
                };
            }
        }
    }
}

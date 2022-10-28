using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moonstorm.Loaders;

namespace Moonstorm
{
    public class MSUTLanguage : LanguageLoader<MSUTLanguage>
    {
        public override string AssemblyDir => MSUTAssets.Instance.AssemblyDir;

        public override string LanguagesFolderName => "MSUTLang";

        internal void Init()
        {
            MSUTLog.Info("Language loader initialized");
            LoadLanguages();
        }
    }
}
using RoR2EditorKit.CodeGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.Editor.CodeGen
{
    public class MainClassCodeGen
    {
        private Writer writer;
        private Data data;
        public Writer WriteCode(out Output output)
        {
            var fileName = Path.GetFileNameWithoutExtension(data.path);
            var className = CSharpCodeHelpers.MakeIdentifier(fileName);
            output = new Output
            {
                mainClassName = className
            };

            CodeGenUtil.WriteHeader(writer, "MainClassCodeGen", "1.0.0");

            WriteUsings();
            writer.WriteLine();
            WriteAssemblyAttributes();
            writer.WriteLine();
            writer.WriteLine($"namespace {data.modName}");
            writer.BeginBlock();

            WriteClassAttributes();
            writer.WriteLine($"public class {className} : BaseUnityPlugin");
            writer.BeginBlock();

            WriteConstants();
            writer.WriteLine($"internal static {className} Instance {{ get; private set; }}");
            writer.WriteLine($"internal static PluginInfo PluginInfo {{ get; private set;}}");
            writer.WriteLine();
            WriteAwake();
            writer.EndBlock();
            writer.EndBlock();
            return writer;
        }

        private void WriteUsings()
        {
            writer.WriteLine("using BepInEx;");
            writer.WriteLine("using R2API;");
            writer.WriteLine("using R2API.Networking;");
            writer.WriteLine("using System;");
            writer.WriteLine("using UnityEngine;");
            writer.WriteLine("using System.Security.Permissions;");
            writer.WriteLine("using System.Security;");
            writer.WriteLine("using UObject = UnityEngine.Object;");
        }

        private void WriteAssemblyAttributes()
        {
            writer.WriteLine("//This attribute enables the SearchableAttribute system to scan your assembly.");
            writer.WriteLine("[assembly: HG.Reflection.SearchableAttribute.OptIn]");
            writer.WriteLine("//The following attributes enables you to reference and use non public members at runtime.");
            writer.WriteLine("#pragma warning disable CS0618");
            writer.WriteLine("[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]");
            writer.WriteLine("#pragma warning restore CS0618");
            writer.WriteLine("[module: UnverifiableCode]");
        }

        private void WriteClassAttributes()
        {
            writer.WriteLine($"[BepInDependency(\"com.TeamMoonstorm.MSU\", BepInDependency.DependencyFlags.HardDependency)]");
            writer.WriteLine($"[BepInPlugin(GUID, MODNAME, VERSION)]");
        }

        private void WriteConstants()
        {
            writer.WriteLine($"public const string GUID = \"com.{data.authorName}.{data.modName}\";");
            writer.WriteLine($"public const string MODNAME = \"{data.humanReadableModName}\";");
            writer.WriteLine($"public const string VERSION = \"0.0.1\";");
        }

        private void WriteAwake()
        {
            writer.WriteLine("//Basic Singleton pattern implementation.");
            writer.WriteLine("private void Awake()");
            writer.BeginBlock();

            writer.WriteLine("Instance = this;");
            writer.WriteLine("PluginInfo = Info;");
            writer.WriteLine("//Initialize logger class.");
            writer.WriteLine($"new {data.loggerClassName}(Logger);");
            writer.EndBlock();
        }
        public MainClassCodeGen(Data data)
        {
            writer = new Writer
            {
                buffer = new StringBuilder()
            };
            this.data = data;
        }

        public struct Data
        {
            public string authorName;
            public string modName;
            public string humanReadableModName;
            public string loggerClassName;

            public string path;
        }

        public struct Output
        {
            public string mainClassName;
        }
    }
}

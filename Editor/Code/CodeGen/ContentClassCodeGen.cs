using RoR2EditorKit.CodeGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonstorm.Editor.CodeGen
{
    public class ContentClassCodeGen
    {
        private Writer writer;
        private Data data;

        public Writer WriteCode(out Output output)
        {
            var fileName = Path.GetFileNameWithoutExtension(data.path);
            var className = CSharpCodeHelpers.MakeIdentifier(fileName);
            output = new Output
            {
                contentClassName = className,
            };

            CodeGenUtil.WriteHeader(writer, "ContentClassCodeGen", "1.0.0");

            WriteUsings();
            writer.WriteLine();

            writer.WriteLine($"namespace {data.modName}");
            writer.BeginBlock();

            writer.EndBlock();
            return writer;
        }

        private void WriteUsings()
        {

        }
        public ContentClassCodeGen(Data data)
        {
            writer = new Writer
            {
                buffer = new StringBuilder()
            };
            this.data = data;
        }
        public struct Data
        {
            public string modName;
            public string mainClassName;
            public string loggerClassName;
            public string assetbundleClassName;
            public bool isSingleAssetsClass;

            public string path;
        }

        public struct Output
        {
            public string contentClassName;
        }
    }
}
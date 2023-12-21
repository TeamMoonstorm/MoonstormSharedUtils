using BepInEx.Logging;
using RoR2EditorKit.CodeGen;
using System;
using System.IO;
using System.Text;

namespace Moonstorm.Editor.CodeGen
{
    public class LoggerClassCodeGen
    {
        private Writer writer;
        private Data data;

        public Writer WriteCode(out Output output)
        {
            var fileName = Path.GetFileNameWithoutExtension(data.path);
            var className = CSharpCodeHelpers.MakeIdentifier(fileName);
            output.loggerClassName = className;

            CodeGenUtil.WriteHeader(writer, "LoggerClassCodeGen", "1.0.0");

            WriteUsings();
            writer.WriteLine();
            writer.WriteLine($"namespace {data.modName}");
            writer.BeginBlock();

            writer.WriteLine($"internal class {className}");
            writer.BeginBlock();
            
            writer.WritePreprocessorDirectiveLine("if DEBUG && !UNITY_EDITOR");
            writer.WriteLine("private static LogLevel _breakableLevel = LogLevel.Fatal;");
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WriteLine();
            writer.WriteLine("private static ManualLogSource _log;");
            writer.WriteLine();
            WriteLogMethods();
            writer.WriteLine("private static void Log(LogLevel level, object data, int i, string member)");
            writer.BeginBlock();

            writer.WritePreprocessorDirectiveLine("if UNITY_EDITOR");

            writer.WriteLine("switch(level)");
            writer.BeginBlock();

            writer.WriteLine("case LogLevel.Debug:");
            writer.WriteLine("case LogLevel.Info:");
            writer.WriteLine("case LogLevel.Message:");
            writer.WriteLine("UDebug.Log(data);");
            writer.WriteLine("break;");
            writer.WriteLine("case LogLevel.Error:");
            writer.WriteLine("case LogLevel.Fatal:");
            writer.WriteLine("UDebug.LogError(Format(data, i, member));");
            writer.WriteLine("break;");
            writer.WriteLine("case LogLevel.Warning:");
            writer.WriteLine("UDebug.LogWarning(Format(data, i, member));");
            writer.WriteLine("break;");
            writer.EndBlock();
            writer.WritePreprocessorDirectiveLine("else");

            writer.WriteLine("object data = (level.HassFlag(LogLevel.Warning) || level.HasFlag(LogLevel.Error) || level.HasFlag(LogLevel.Fatal)) ? Format(data, i, member) : data");
            writer.WriteLine();
            writer.WriteLine("_log.Log(level, data);");
            writer.WritePreprocessorDirectiveLine("if DEBUG");

            writer.WriteLine("if(_breakableLevel.HasFlag(level))");
            writer.BeginBlock();

            writer.WriteLine("TryBreak();");
            writer.EndBlock();
            writer.WritePreprocessorDirectiveLine("endif");
            writer.WritePreprocessorDirectiveLine("endif");
            writer.EndBlock();

            writer.WriteLine();
            writer.WriteLine("public static string Format(object data, int i, string member)");
            writer.BeginBlock();

            writer.WriteLine("return $\"{data} | Line={i} : Member={member}\";");
            writer.EndBlock();
            writer.WriteLine();

            writer.WritePreprocessorDirectiveLine("if DEBUG");

            writer.WriteLine("private static void TryBreak()");
            writer.BeginBlock();

            writer.WriteLine("if (Debugger.IsAttached)");
            writer.BeginBlock();

            writer.WriteLine("Debugger.Break();");
            writer.EndBlock();

            writer.EndBlock();
            writer.WritePreprocessorDirectiveLine("endif");

            writer.WriteLine();
            writer.WriteLine($"internal {className}(ManualLogSource log)");
            writer.BeginBlock();

            writer.WriteLine("_log = log;");
            writer.EndBlock();
            writer.EndBlock();
            writer.EndBlock();

            return writer;
        }

        private void WriteUsings()
        {
            writer.WriteLine(
@"using BepInEx.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UDebug = UnityEngine.Debug;");
        }

        private void WriteLogMethods()
        {
            foreach(string logLevel in Enum.GetNames(typeof(LogLevel)))
            {
                if (logLevel == "All" || logLevel == "None")
                    continue;

                writer.WriteLine($"public static void {logLevel}(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = \"\") => Log(LogLevel.{logLevel}, data, i, member);");
                writer.WriteLine();
            }
        }

        public LoggerClassCodeGen(Data data)
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

            public string path;
        }

        public struct Output
        {
            public string loggerClassName;
        }
    }
}

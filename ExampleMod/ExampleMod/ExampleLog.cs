using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UDebug = UnityEngine.Debug;

namespace ExampleMod
{
    //This class is used for logging data related to our mod, we should ideally utilize this class instead of
    //UnityEngine.Debug as it has a couple of extra features not present in the regular UnityEngine.Debug class.
    internal class ExampleLog
    {
        //This becomes our mod's Logger, this is assigned during the constructor for this class.
        private static ManualLogSource logger = null;

        //Sometimes it may be useful to make a Debugger break when a certain level of Log gets logged, This field
        //allows us to set a level of Log, at which if hit, the debugger will break so we can inspect the runtime code.
        //This only gets compiled on debug mode
#if DEBUG
        private static LogLevel _breakableLevel = LogLevel.Fatal;
#endif

        //The following 6 methods are Log methods, which are used to log messages into the Log file and the Console.
        //Keep in mind that the second and third arguments should not be filled manually, as these are filled automatically by the compiler at runtime.
        public static void Fatal(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Fatal, data, i, member);

        public static void Error(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Error, data, i, member);

        public static void Warning(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Warning, data, i, member);

        public static void Message(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Message, data, i, member);

        public static void Info(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Info, data, i, member);

        public static void Debug(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "") => Log(LogLevel.Debug, data, i, member);

        //This is the internal Log method, It uses conditional compilation to use different code depending on if this
        //gets called in the Unity Editor or at Runtime.
        private static void Log(LogLevel level, object data, int i, string member)
        {
#if UNITY_EDITOR
            LogEditor(level, data, i, member);
#else
            LogRuntime(level, data, i, member);
#endif
        }

        //This instead of using our mod's logger, which is not available at the Editor level, uses the Debug class from
        //Unity to log the message, making it appear on the UnityEditor's console.
        private static void LogEditor(LogLevel level, object data, int i, string member)
        {
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                case LogLevel.Message:
                    UDebug.Log(Format(data, i, member));
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    UDebug.LogError(Format(data, i, member));
                    break;
                case LogLevel.Warning:
                    UDebug.LogWarning(Format(data, i, member));
                    break;
            }
        }

        //This method only gets called at the runtime level, despite there being no references, do not remove this code.
        //It also tries to break the debugger if compiled in Debug mode.
        private static void LogRuntime(LogLevel level, object data, int i, string member)
        {
            object data2 = (level.HasFlag(LogLevel.Warning) || level.HasFlag(LogLevel.Error) || level.HasFlag(LogLevel.Fatal)) ? Format(data, i, member) : data;

            logger.Log(level, data2);
#if DEBUG
            if (_breakableLevel.HasFlag(level))
            {
                TryBreak();
            }
#endif
        }

        //Method that formats our object data into a readable string, suffixes the string with this class's name if ran
        //in the Editor.
        public static string Format(object data, int i, string member)
        {
#if UNITY_EDITOR
            return $"['{nameof(ExampleLog)}')]: {data} | Line={i} : Member={member}";
#else
            return $"{data} | Line={i} : Member={member}";
#endif
        }

        //This tries to break the debugger, if one is attached.
#if DEBUG
        private static void TryBreak()
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }
#endif

        //Constructor which is used to set the logger.
        internal ExampleLog(ManualLogSource logger_)
        {
            logger = logger_;
        }
    }
}
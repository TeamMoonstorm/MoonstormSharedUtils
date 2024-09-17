using BepInEx.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UDebug = UnityEngine.Debug;

namespace Moonstorm.Loaders
{
    public abstract class LogLoader<T> : LogLoader where T : LogLoader<T>
    {

        public static T Instance { get; private set; }

        public LogLoader(ManualLogSource logSource) : base(logSource)
        {
            try
            {
                if (Instance != null)
                {
                    throw new InvalidOperationException($"Singleton class \"{typeof(T).Name}\" inheriting LogLoader was instantiated twice.");
                }
                Instance = this as T;
            }
            catch (Exception e) { MSULog.Error(e); }
        }

        public static void Message(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if UNITY_EDITOR
            UDebug.Log(DefaultFormat(data, i, member));
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            ThrowIfNoInstance("Log Message");
#pragma warning restore CS0162 // Unreachable code detected
            Instance.LogMessage(data, i, member);
        }

        public static void Info(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if UNITY_EDITOR
            UDebug.Log(DefaultFormat(data, i, member));
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            ThrowIfNoInstance("Log Info");
#pragma warning restore CS0162 // Unreachable code detected
            Instance.LogInfo(data, i, member);
        }

        public static void Debug(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if UNITY_EDITOR
            UDebug.Log(DefaultFormat(data, i, member));
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            ThrowIfNoInstance("Log Debug");
#pragma warning restore CS0162 // Unreachable code detected
            Instance.LogDebug(data, i, member);
        }

        public static void Warning(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if UNITY_EDITOR
            UDebug.LogWarning(DefaultFormat(data, i, member));
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            ThrowIfNoInstance("Log Warning");
#pragma warning restore CS0162 // Unreachable code detected
            Instance.LogWarning(data, i, member);
        }

        public static void Error(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if UNITY_EDITOR
            UDebug.LogError(DefaultFormat(data, i, member));
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            ThrowIfNoInstance("Log Error");
#pragma warning restore CS0162 // Unreachable code detected
            Instance.LogError(data, i, member);
        }

        public static void Fatal(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if UNITY_EDITOR
            UDebug.LogError(DefaultFormat(data, i, member));
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            ThrowIfNoInstance("Log Fatal");
#pragma warning restore CS0162 // Unreachable code detected
            Instance.LogFatal(data, i, member);
        }

        protected static void ThrowIfNoInstance(string attemptedAction)
        {
#if !UNITY_EDITOR
            if(Instance == null)
                throw new NullReferenceException($"Cannot {attemptedAction} when there's no instance of {typeof(T).Name}!");
#endif
        }
    }

    public abstract class LogLoader
    {
        [Flags]
        public enum BreakOnLog
        {
            None = 0,
            Message = 1,
            Info = 2,
            Debug = 4,
            Warning = 8,
            Error = 16,
            Fatal = 32,
            Everything = ~None,
        };

        public abstract ManualLogSource LogSource { get; protected set; }

        public abstract BreakOnLog BreakOn { get; }

        public virtual void LogMessage(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if DEBUG
            LogSource.LogMessage(FormatString(data, i, member));
            if (BreakOn.HasFlag(BreakOnLog.Message))
            {
                TryBreak();
            }
#else
            LogSource.LogMessage(data);
#endif
        }

        public virtual void LogInfo(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if DEBUG
            LogSource.LogInfo(FormatString(data, i, member));
            if (BreakOn.HasFlag(BreakOnLog.Info))
            {
                TryBreak();
            }
#else
            LogSource.LogInfo(data);
#endif
        }

        public virtual void LogDebug(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            LogSource.LogDebug(FormatString(data, i, member));
#if DEBUG
            if (BreakOn.HasFlag(BreakOnLog.Debug))
            {
                TryBreak();
            }
#endif
        }

        public virtual void LogWarning(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if DEBUG
            LogSource.LogWarning(FormatString(data, i, member));
            if (BreakOn.HasFlag(BreakOnLog.Warning))
            {
                TryBreak();
            }
#else
            LogSource.LogWarning(data);
#endif
        }

        public virtual void LogError(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            LogSource.LogError(FormatString(data, i, member));
#if DEBUG
            if (BreakOn.HasFlag(BreakOnLog.Error))
            {
                TryBreak();
            }
#endif
        }

        public virtual void LogFatal(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            LogSource.LogFatal(FormatString(data, i, member));
#if DEBUG
            if (BreakOn.HasFlag(BreakOnLog.Fatal))
            {
                TryBreak();
            }
#endif
        }

        public virtual string FormatString(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            return DefaultFormat(data, i, member);
        }

        protected static string DefaultFormat(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            return string.Format("{0} :: Line: {1}, Method {2}", data, i, member);
        }

#if DEBUG
        private static void TryBreak()
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }
#endif

        public LogLoader(ManualLogSource logSource)
        {
            LogSource = logSource;
        }
    }
}
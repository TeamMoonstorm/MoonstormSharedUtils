using BepInEx.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UDebug = UnityEngine.Debug;

namespace Moonstorm.Loaders
{
    /// <summary>
    /// The LogLoader is a class that can be used to Log what happens during your mod's lifetime.
    /// <para>The LogLoader will work both on the UnityEditor and During the normal runtime of your application.</para>
    /// <para>LogLoader inheriting classes are treated as Singletons</para>
    /// </summary>
    /// <typeparam name="T">The class that's inheriting from LogLoader</typeparam>
    public abstract class LogLoader<T> : LogLoader where T : LogLoader<T>
    {
        /// <summary>
        /// Retrieves the instance of <typeparamref name="T"/>
        /// </summary>
        public static T Instance { get; private set; }

        /// <summary>
        /// Constructor for LogLoader, this will throw an invalid operation exception if an instance of <typeparamref name="T"/> already exists.
        /// </summary>
        public LogLoader(ManualLogSource logSource) : base(logSource)
        {
            try
            {
                if(Instance != null)
                {
                    throw new InvalidOperationException($"Singleton class \"{typeof(T).Name}\" inheriting LogLoader was instantiated twice.");
                }
            }
            catch (Exception e) { MSULog.Error(e); }
        }

        /// <summary>
        /// Quick method for Logging <paramref name="data"/> on a Message level.
        /// <br>If used in the UnityEditor, it Logs using the default Unity Debug.Log() method.</br>
        /// <br>Outside of the UnityEditor, it requires an <see cref="Instance"/> of <typeparamref name="T"/> to exist</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
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

        /// <summary>
        /// Quick method for Logging <paramref name="data"/> on an Info level.
        /// <br>If used in the UnityEditor, it Logs using the default Unity Debug.Log() method.</br>
        /// <br>Outside of the UnityEditor, it requires an <see cref="Instance"/> of <typeparamref name="T"/> to exist</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
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

        /// <summary>
        /// Quick method for Logging <paramref name="data"/> on a Debug level.
        /// <br>If used in the UnityEditor, it Logs using the default Unity Debug.Log() method.</br>
        /// <br>Outside of the UnityEditor, it requires an <see cref="Instance"/> of <typeparamref name="T"/> to exist</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
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

        /// <summary>
        /// Quick method for Logging <paramref name="data"/> on a Warning level.
        /// <br>If used in the UnityEditor, it Logs using the default Unity Debug.LogWarning() method.</br>
        /// <br>Outside of the UnityEditor, it requires an <see cref="Instance"/> of <typeparamref name="T"/> to exist</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
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

        /// <summary>
        /// Quick method for Logging <paramref name="data"/> on an Error level.
        /// <br>If used in the UnityEditor, it Logs using the default Unity Debug.LogError() method.</br>
        /// <br>Outside of the UnityEditor, it requires an <see cref="Instance"/> of <typeparamref name="T"/> to exist</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
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

        /// <summary>
        /// Quick method for Logging <paramref name="data"/> on a Fatal level.
        /// <br>If used in the UnityEditor, it Logs using the default Unity Debug.LogERror() method.</br>
        /// <br>Outside of the UnityEditor, it requires an <see cref="Instance"/> of <typeparamref name="T"/> to exist</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
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

        /// <summary>
        /// Throws a null reference exception if no instance is found. use with caution
        /// </summary>
        protected static void ThrowIfNoInstance(string attemptedAction)
        {
#if !UNITY_EDITOR
            if(Instance == null)
                throw new NullReferenceException($"Cannot {attemptedAction} when there's no instance of {typeof(T).Name}!");
#endif
        }
    }

    /// <summary>
    /// <inheritdoc cref="ConfigLoader{T}"/>
    /// <para>You probably want to use <see cref="ConfigLoader{T}"/> instead.</para>
    /// </summary>
    public abstract class LogLoader
    {
        /// <summary>
        /// A Flags enum which tells the default implementations of this class's methods wether to break or not.
        /// <br>For more information, see <see cref="BreakOn"/></br>
        /// </summary>
        [Flags]
        public enum BreakOnLog
        {
            /// <summary>
            /// No Level of Debugging triggers a break
            /// </summary>
            None = 0,
            /// <summary>
            /// Message Level Debugging triggers a break
            /// </summary>
            Message = 1,
            /// <summary>
            /// Info Level Debugging triggers a break
            /// </summary>
            Info = 2,
            /// <summary>
            /// Debug Level Debugging triggers a break
            /// </summary>
            Debug = 4,
            /// <summary>
            /// Warning Level Debugging triggers a break
            /// </summary>
            Warning = 8,
            /// <summary>
            /// Error Level Debugging triggers a break
            /// </summary>
            Error = 16,
            /// <summary>
            /// Fatal Level Debugging triggers a break
            /// </summary>
            Fatal = 32,
            /// <summary>
            /// All Levels Debugging triggers a break
            /// </summary>
            Everything = ~None,
        };

        /// <summary>
        /// Your MainClass's Logger.
        /// </summary>
        public abstract ManualLogSource LogSource { get; protected set; }

        /// <summary>
        /// When building MSU on Debug mode, depending on the flags set here, the default implementations of the following methods will try to Break if a debugger is attached.
        /// <list type="bullet">
        /// <item>
        /// <see cref="LogMessage(object, int, string)"/> requires the <see cref="BreakOnLog.Message"/> flag
        /// </item>
        /// <item>
        /// <see cref="LogInfo(object, int, string)"/> requires the <see cref="BreakOnLog.Info"/> flag
        /// </item>
        /// <item>
        /// <see cref="LogDebug(object, int, string)"/> requires the <see cref="BreakOnLog.Debug"/> flag
        /// </item>
        /// <item>
        /// <see cref="LogWarning(object, int, string)"/> requires the <see cref="BreakOnLog.Warning"/> flag
        /// </item>
        /// <item>
        /// <see cref="LogError(object, int, string)"/> requires the <see cref="BreakOnLog.Error"/> flag
        /// </item>
        /// <item>
        /// <see cref="LogFatal(object, int, string)"/> requires the <see cref="BreakOnLog.Fatal"/> flag
        /// </item>
        /// </list>
        /// </summary>
        public abstract BreakOnLog BreakOn { get; }


        /// <summary>
        /// Logs <paramref name="data"/> on a Message level using the <see cref="ManualLogSource"/> specified in <see cref="LogSource"/>
        /// <br>In debug mode, if <see cref="BreakOn"/> has the flag <see cref="BreakOnLog.Message"/>, and a debugger is attached, the debugger will break.</br>
        /// <br>This method is Overridable</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
        public virtual void LogMessage(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if DEBUG
            LogSource.LogMessage(FormatString(data, i, member));
            if(BreakOn.HasFlag(BreakOnLog.Message))
            {
                TryBreak();
            }
#else
            LogSource.LogMessage(data);
#endif
        }

        /// <summary>
        /// Logs <paramref name="data"/> on an Info level using the <see cref="ManualLogSource"/> specified in <see cref="LogSource"/>
        /// <br>In debug mode, if <see cref="BreakOn"/> has the flag <see cref="BreakOnLog.Info"/>, and a debugger is attached, the debugger will break.</br>
        /// <br>This method is Overridable</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
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

        /// <summary>
        /// Logs <paramref name="data"/> on a Debug level using the <see cref="ManualLogSource"/> specified in <see cref="LogSource"/>
        /// <br>In debug mode, if <see cref="BreakOn"/> has the flag <see cref="BreakOnLog.Debug"/>, and a debugger is attached, the debugger will break.</br>
        /// <br>This method is Overridable</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
        public virtual void LogDebug(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            LogSource.LogDebug(FormatString(data, i, member));
#if DEBUG
            if(BreakOn.HasFlag(BreakOnLog.Debug))
            {
                TryBreak();
            }
#endif
        }

        /// <summary>
        /// Logs <paramref name="data"/> on a Warning level using the <see cref="ManualLogSource"/> specified in <see cref="LogSource"/>
        /// <br>In debug mode, if <see cref="BreakOn"/> has the flag <see cref="BreakOnLog.Warning"/>, and a debugger is attached, the debugger will break.</br>
        /// <br>This method is Overridable</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
        public virtual void LogWarning(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
#if DEBUG
            LogSource.LogWarning(FormatString(data, i, member));
            if(BreakOn.HasFlag(BreakOnLog.Warning))
            {
                TryBreak();
            }
#else
            LogSource.LogWarning(data);
#endif
        }

        /// <summary>
        /// Logs <paramref name="data"/> on an Error level using the <see cref="ManualLogSource"/> specified in <see cref="LogSource"/>
        /// <br>In debug mode, if <see cref="BreakOn"/> has the flag <see cref="BreakOnLog.Error"/>, and a debugger is attached, the debugger will break.</br>
        /// <br>This method is Overridable</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
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

        /// <summary>
        /// Logs <paramref name="data"/> on a Fatal level using the <see cref="ManualLogSource"/> specified in <see cref="LogSource"/>
        /// <br>In debug mode, if <see cref="BreakOn"/> has the flag <see cref="BreakOnLog.Fatal"/>, and a debugger is attached, the debugger will break.</br>
        /// <br>This method is Overridable</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
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

        /// <summary>
        /// Method used to format the <paramref name="data"/> to a human readable format.
        /// <br>By default it uses the method <see cref="DefaultFormat(object, int, string)"/></br>
        /// <br>This method is Overridable</br>
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
        /// <returns>A human readable string</returns>
        public virtual string FormatString(object data, [CallerLineNumber] int i = 0, [CallerMemberName] string member = "")
        {
            return DefaultFormat(data, i, member);
        }

        /// <summary>
        /// Method used to format the <paramref name="data"/> to a human readable format.
        /// </summary>
        /// <param name="data">The data to log</param>
        /// <param name="i">The line of code where this method was called, this is provided automatically and should not be overwritten</param>
        /// <param name="member">The name of the method that called this method, this is providedd automatically and should not be overwritten</param>
        /// <returns>A human readable string</returns>
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

        /// <summary>
        /// Constructor for a LogLoader
        /// </summary>
        /// <param name="logSource">The LogSource where the logging will be done.</param>
        public LogLoader(ManualLogSource logSource)
        {
            LogSource = logSource;
        }
    }
}
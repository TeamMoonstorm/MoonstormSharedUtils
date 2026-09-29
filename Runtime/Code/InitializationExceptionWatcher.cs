using BepInEx;
using BepInEx.Logging;
using RoR2;
using RoR2.UI;
using RoR2.UI.MainMenu;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MSU
{
    public static class InitializationExceptionWatcher
    {
        static InitializationExceptionWatcher()
        {
            MainMenuController.OnMainMenuInitialised += ShowDialogBoxIfApplicable;
        }

        public static bool exceptionsThrown => _pluginToExceptions.Count > 0;
        private static Dictionary<BaseUnityPlugin, List<Exception>> _pluginToExceptions = new Dictionary<BaseUnityPlugin, List<Exception>>();

        private static void ShowDialogBoxIfApplicable()
        {
            MainMenuController.OnMainMenuInitialised -= ShowDialogBoxIfApplicable;

            if(!exceptionsThrown)
            {
#if DEBUG
                MSULog.Info("No exceptions thrown, not showing initialization error helpbox.");
#endif
                return;
            }

            //"Exception during Module Initialization"
            var header = new SimpleDialogBox.TokenParamsPair
            {
                token = "MSU_INIT_ERROR_DIALOG_BOX_HEADER",
                formatParams = Array.Empty<object>()
            };

            //A total of "x" Mod(s) threw exceptions during MSU's module initialization procedure. The game may be in an unstable state.
            //ModName - NumberOfExceptions
            var description = new SimpleDialogBox.TokenParamsPair
            {
                token = "MSU_INIT_ERROR_DIALOG_BOX_DESC",
                formatParams = GetDialogBoxDescFormat()
            };

            var box = SimpleDialogBox.Create();
            box.descriptionToken = description;
            box.headerToken = header;

            box.AddCancelButton("MSU_INIT_ERROR_DIALOG_BOX_CANCEL");
            box.AddCommandButton("quit", "MSU_INIT_ERROR_DIALOG_BOX_QUIT");

            box.rootObject.transform.SetParent(RoR2Application.instance.mainCanvas.transform);
        }

        public static void TryCatch(Action method, BaseUnityPlugin baseUnityPlugin, ManualLogSource logSource = null)
        {
            logSource ??= MSULog._logSource;

            try
            {
                method();
            }
            catch (Exception ex)
            {
                logSource.LogError(ex);
                OnExceptionThrown(ex, baseUnityPlugin);
            }
        }

        public static IEnumerator TryCatch(IEnumerator coroutine, BaseUnityPlugin baseUnityPlugin, ManualLogSource logSource)
        {
            while(true)
            {
                bool hasWork = false;
                try
                {
                    hasWork = coroutine.MoveNext();
                }
                catch(Exception ex)
                {
                    logSource.LogError(ex);
                    OnExceptionThrown(ex, baseUnityPlugin);
                }

                yield return null;

                if (hasWork == false)
                    break;
            }
        }

        private static object[] GetDialogBoxDescFormat()
        {
            using var _ = HG.StringBuilderPool.RentStringBuilder(out var stringBuilder);

            foreach (var (plugin, exceptions) in _pluginToExceptions)
            {
                var metadata = plugin.Info.Metadata;
                stringBuilder.Append($"{metadata.Name}({metadata.GUID}) | {exceptions.Count} ");

                if(exceptions.Count > 1)
                {
                    stringBuilder.AppendLine("Exceptions");
                }
                else
                {
                    stringBuilder.AppendLine("Exception");
                }
            }

            return new object[]
            {
                _pluginToExceptions.Count,
                stringBuilder.ToString()
            };
        }

        public static void AddException(Exception ex, BaseUnityPlugin plugin)
        {
            OnExceptionThrown(ex, plugin);
        }

        private static void OnExceptionThrown(Exception ex, BaseUnityPlugin plugin)
        {
            if(!_pluginToExceptions.TryGetValue(plugin, out var exceptionList))
            {
                exceptionList = new List<Exception>();
                _pluginToExceptions.Add(plugin, exceptionList);
            }

            exceptionList.Add(ex);
        }
    }
}
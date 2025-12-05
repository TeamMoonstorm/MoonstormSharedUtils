using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MSU
{
    /// <summary>
    /// Static class used for loading a mod's language files
    /// </summary>
    public static class LanguageFileLoader
    {
        internal static Dictionary<string, Dictionary<string, string>> _languageNameToRawTokenData = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds new language files from the specified mod to the LanguageFileLoader
        /// <br>These files are then parsed from JSON and then get added to the Language at runtime after being formatted by the <see cref="FormatTokenManager"/></br>
        /// </summary>
        /// <param name="baseUnityPlugin">The mod that's adding the new language files</param>
        /// <param name="languageFolderName">The folder name that contains the new Language files, relative to <paramref name="baseUnityPlugin"/>'s location (<see cref="BaseUnityPlugin.Info"/> -> <see cref="PluginInfo.Location"/></param>
        public static void AddLanguageFilesFromMod(BaseUnityPlugin baseUnityPlugin, string languageFolderName)
        {
            if(!CheckForDirectory(baseUnityPlugin, languageFolderName, out var directory))
            {
#if DEBUG
                MSULog.Warning($"No directory exists in {directory}, cannot add language files.");
#endif
                return;
            }

            var languageNameToJSONPaths = GetLanguageNameToJSONPaths(directory);
            ReadTextAndAddTokens(languageNameToJSONPaths);
        }

        /// <summary>
        /// Asynchronously adds new language files from the specified mod to the LanguageFileLoader
        /// <br>These files are then parsed from JSON and then get added to the Language at runtime after being formatted by the <see cref="FormatTokenManager"/></br>
        /// </summary>
        /// <param name="baseUnityPlugin">The mod that's adding the new language files</param>
        /// <param name="languageFolderName">The folder name that contains the new Language files, relative to <paramref name="baseUnityPlugin"/>'s location (<see cref="BaseUnityPlugin.Info"/> -> <see cref="PluginInfo.Location"/></param>
        /// <returns>A coroutine that can be awaited</returns>
        public static IEnumerator AddLanguageFilesFromModAsync(BaseUnityPlugin baseUnityPlugin, string languageFoldeName)
        {
            if (!CheckForDirectory(baseUnityPlugin, languageFoldeName, out var directory))
            {
#if DEBUG
                MSULog.Warning($"No directory exists in {directory}, cannot add language files.");
#endif
                yield break;
            }

            var languageNameToJSONPaths = GetLanguageNameToJSONPaths(directory);
            var subroutine = ReadTextAndAddTokensAsync(languageNameToJSONPaths);
            while (!subroutine.IsDone())
                yield return null;

        }

        private static bool CheckForDirectory(BaseUnityPlugin baseUnityPlugin, string languageFolderName, out string directory)
        {
            var dirName = Path.GetDirectoryName(baseUnityPlugin.Info.Location);
            directory = Path.Combine(dirName, languageFolderName);
            return Directory.Exists(directory);
        }

        private static Dictionary<string, List<string>> GetLanguageNameToJSONPaths(string languageFolderDirectory)
        {
            var result = new Dictionary<string, List<string>>();
            var newDirectories = Directory.EnumerateDirectories(languageFolderDirectory);
            foreach(var languageDirectoryPath in newDirectories)
            {
                string languageName = Path.GetFileNameWithoutExtension(languageDirectoryPath);
                if(!result.ContainsKey(languageName))
                {
                    result.Add(languageName, new List<string>());
                }

                result[languageName].AddRange(Directory.EnumerateFiles(languageDirectoryPath, "*.json"));
            }
            return result;
        }

        private static void ReadTextAndAddTokens(Dictionary<string, List<string>> languageToJSONPaths)
        {
            foreach (var (languageName, jsonFiles) in languageToJSONPaths)
            {
                if (!_languageNameToRawTokenData.ContainsKey(languageName))
                {
                    _languageNameToRawTokenData.Add(languageName, new Dictionary<string, string>());
                }

                var dictForLang = _languageNameToRawTokenData[languageName];
                foreach (var jsonFile in jsonFiles)
                {
                    var jsonText = File.ReadAllText(jsonFile);
                    try
                    {
                        JSONLanguageFile languageFile = JsonConvert.DeserializeObject<JSONLanguageFile>(jsonText);

                        foreach (var (token, value) in languageFile.strings)
                        {
                            if (!dictForLang.ContainsKey(token))
                            {
                                dictForLang[token] = value;
                                continue;
                            }
#if DEBUG
                            MSULog.Warning($"While enumerating language files for the LanguageFileLoader, the file {jsonFile} is trying to add a token with the key {token}. However, that token already exists in the language name {languageName}!");
#endif
                        }
                    }
                    catch (Exception e)
                    {
                        MSULog.Error($"Error while parsing JSON for language file {jsonFile}!\n{e}");
                    }
                }
            }
        }

        private static IEnumerator ReadTextAndAddTokensAsync(Dictionary<string, List<string>> languageToJSONPaths)
        {
            foreach (var (languageName, jsonFiles) in languageToJSONPaths)
            {
                if (!_languageNameToRawTokenData.ContainsKey(languageName))
                {
                    _languageNameToRawTokenData.Add(languageName, new Dictionary<string, string>());
                }

                var dictForLang = _languageNameToRawTokenData[languageName];
                StringContainer[] containers = new StringContainer[jsonFiles.Count];
                ParallelCoroutine coroutine = new ParallelCoroutine();
                for(int i = 0; i <  jsonFiles.Count; i++)
                {
                    containers[i] = new StringContainer();
                    coroutine.Add(ReadTextAsync(jsonFiles[i], containers[i]));
                }

                while (!coroutine.isDone)
                    yield return null;

                var jsonTexts = containers.Select(c => c.value).ToArray();
                for(int i = 0; i < jsonTexts.Length; i++)
                {
                    string jsonText = jsonTexts[i];
                    try
                    {
                        JSONLanguageFile languageFile = JsonConvert.DeserializeObject<JSONLanguageFile>(jsonText);

                        foreach (var (token, value) in languageFile.strings)
                        {
                            if (!dictForLang.ContainsKey(token))
                            {
                                dictForLang[token] = value;
                                continue;
                            }
#if DEBUG
                            MSULog.Warning($"While enumerating language files for the LanguageFileLoader, the file {jsonFiles[i]} is trying to add a token with the key {token}. However, that token already exists in the language name {languageName}!");
#endif
                        }
                    }
                    catch (Exception e)
                    {
                        MSULog.Error($"Error while parsing JSON for language file {jsonFiles[i]}!\n{e}");
                    }
                }
            }

            IEnumerator ReadTextAsync(string path, StringContainer output)
            {
                var task = File.ReadAllTextAsync(path);
                while (!task.IsCompleted)
                    yield return null;

                output.value = task.Result;
                yield break;
            }
        }

        private class StringContainer
        {
            public string value;
        }

        [Serializable]
        private class JSONLanguageFile
        {
            public Dictionary<string, string> strings { get; set; }
        }
    }
}

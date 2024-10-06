using MSU.Config;
using RiskOfOptions.Components.Panel;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SearchableAttribute = HG.Reflection.SearchableAttribute;

namespace MSU
{
    internal static class FormatTokenManager
    {
        private static Dictionary<string, FormatTokenAttribute[]> _cachedFormattingArray = null;

        [RoR2.SystemInitializer(typeof(ConfigSystem))]
        private static IEnumerator Init()
        {
            MSULog.Info("Initializing FormatTokenManager");
            var subroutine = CreateFormattingArray();

            while (!subroutine.IsDone())
            {
                yield return null;
            }

            foreach(var lang in Language.GetAllLanguages())
            {
#if DEBUG
                MSULog.Info($"Adding tokens for language {lang}");
#endif
                AddTokensFromLanguageFileLoaderAndFormatThem(lang);
            }
            RoR2.Language.onCurrentLanguageChanged += Language_onCurrentLanguageChanged;

            ModOptionPanelController.OnModOptionsExit += () =>
            {
                MSULog.Info($"Reformatting Tokens.");

                AddTokensFromLanguageFileLoaderAndFormatThem(RoR2.Language.currentLanguage);
            };
        }

        private static void Language_onCurrentLanguageChanged()
        {
            MSULog.Info($"Changed to language {RoR2.Language.currentLanguageName}.");

            AddTokensFromLanguageFileLoaderAndFormatThem(RoR2.Language.currentLanguage);
        }

        private static void AddTokensFromLanguageFileLoaderAndFormatThem(RoR2.Language currentLanguage)
        {
            var langName = currentLanguage.name;
            //See if we have custom tokens to add
            if(LanguageFileLoader._languageNameToRawTokenData.TryGetValue(langName, out var rawtokenData))
            {
                List<(string, string)> tokensToFormatOnRoutine = new List<(string, string)>();
                foreach(var (token, value) in rawtokenData)
                {

                    //If there's a formatting option for this token, add it to a list we'll later process on a coroutine. check method coroutine for more info
                    if(_cachedFormattingArray.TryGetValue(token, out var cachedFormattingArray))
                    {
                        tokensToFormatOnRoutine.Add((token, value));
                        continue;
                    }
                    //Otherwise just ensure the token exists.
                    currentLanguage.stringsByToken[token] = value;
                }
                if(tokensToFormatOnRoutine.Count > 0)
                {
                    MSUMain.instance.StartCoroutine(FormatTokensWhenConfigsAreBound(currentLanguage, tokensToFormatOnRoutine));
                }
                return;
            }
#if DEBUG
            MSULog.Info($"No tokens are available on language {langName}...");
#endif
        }

        
        // We need to wait for the config system to be bound because most of the time FormatToken attributes are assigned to fields that end up being configured.
        private static IEnumerator FormatTokensWhenConfigsAreBound(Language target, List<(string, string)> tokenValuePair)
        {
            while (!ConfigSystem.configsBound)
            {
                yield return null;
            }

            foreach (var (token, value) in tokenValuePair)
            {
                try
                {
                    target.SetStringByToken(token, FormatString(token, value, _cachedFormattingArray[token]));
                }
                catch(Exception e)
                {
                    MSULog.Error($"Failed to format string value for token {token} in language {target.name}. unformatted string will be used.\n{e}");
                    target.SetStringByToken(token, value);
                }
            }
        }

        private static IEnumerator CreateFormattingArray()
        {
            List<FormatTokenAttribute> propertyFormatTokens = new List<FormatTokenAttribute>();
            List<FormatTokenAttribute> fieldFormatTokens = new List<FormatTokenAttribute>();

            var subroutine = GetFormatTokenLists(propertyFormatTokens, fieldFormatTokens);
            while (!subroutine.IsDone())
                yield return null;

            Dictionary<string, FormatTokenAttribute[]> formattingDictionaryFromFields = new Dictionary<string, FormatTokenAttribute[]>();
            Dictionary<string, FormatTokenAttribute[]> formattingDictionaryFromProperties = new Dictionary<string, FormatTokenAttribute[]>();

            var parallelSubroutine = new ParallelCoroutine();
            parallelSubroutine.Add(CreateFormattingDictionary(propertyFormatTokens, formattingDictionaryFromProperties));
            parallelSubroutine.Add(CreateFormattingDictionary(fieldFormatTokens, formattingDictionaryFromFields));

            while (!parallelSubroutine.IsDone())
                yield return null;

            _cachedFormattingArray = new Dictionary<string, FormatTokenAttribute[]>();

            foreach (var (token, formattingArray) in formattingDictionaryFromFields)
            {
                yield return null;
                //Add token from dictionary, this replaces the array, but that's ok as this dictionary is currently empty
                _cachedFormattingArray[token] = Array.Empty<FormatTokenAttribute>();
                var arrayFromCache = _cachedFormattingArray[token];
                for (int i = 0; i < formattingArray.Length; i++)
                {
                    yield return null;
                    //Resize if needed
                    if (arrayFromCache.Length < i + 1)
                    {
                        Array.Resize(ref arrayFromCache, i + 1);
                    }

                    //only set value if the value in the cache is not null
                    if (arrayFromCache[i] == null)
                        arrayFromCache[i] = formattingArray[i];
                }
                _cachedFormattingArray[token] = arrayFromCache;
            }
            foreach (var (token, formattingArray) in formattingDictionaryFromProperties)
            {
                yield return null;
                //We do not overwrite the array if the token is already in the dictionary.
                //This is due to the fact that the kye may already be in the dictionary due to being created from fields with the token modifiers

                if (!_cachedFormattingArray.ContainsKey(token))
                {
                    _cachedFormattingArray[token] = Array.Empty<FormatTokenAttribute>();
                }
                var arrayFromCache = _cachedFormattingArray[token];
                for (int i = 0; i < formattingArray.Length; i++)
                {
                    yield return null;
                    if (arrayFromCache.Length < i + 1)
                    {
                        Array.Resize(ref arrayFromCache, i + 1);
                    }
                    //only set value if the value in the cache is not null 
                    if (arrayFromCache[i] == null)
                        arrayFromCache[i] = formattingArray[i];
                }
                _cachedFormattingArray[token] = arrayFromCache;
            }
        }

        private static IEnumerator GetFormatTokenLists(List<FormatTokenAttribute> propertyFormatTokens, List<FormatTokenAttribute> fieldFormatTokens)
        {
            var allTokenModifiers = SearchableAttribute.GetInstances<FormatTokenAttribute>() ?? new List<SearchableAttribute>();
            foreach (FormatTokenAttribute formatToken in allTokenModifiers.Cast<FormatTokenAttribute>())
            {
                yield return null;
                if (formatToken.target is FieldInfo)
                {
                    fieldFormatTokens.Add(formatToken);
                }
                else
                {
                    propertyFormatTokens.Add(formatToken);
                }
            }
        }

        private static IEnumerator CreateFormattingDictionary(List<FormatTokenAttribute> source, Dictionary<string, FormatTokenAttribute[]> dest)
        {
            if (source.Count == 0)
                yield break;

            foreach (FormatTokenAttribute formatToken in source)
            {
                yield return null;
                try
                {
                    var token = formatToken.languageToken;
                    var formattingIndex = formatToken.formattingIndex;
                    //If the token is not in the dictionary, add it and initialize an empty array.
                    if (!dest.ContainsKey(token))
                    {
                        dest[token] = Array.Empty<FormatTokenAttribute>();
                    }

                    var dictArray = dest[token];
                    //Ensure array is big enough for the new modifier
                    if (dictArray.Length < formattingIndex + 1)
                    {
                        Array.Resize(ref dictArray, formattingIndex + 1);
                    }

                    //We should only set the modifier if there is no modifier already
                    if (dictArray[formattingIndex] == null)
                    {
                        dictArray[formattingIndex] = formatToken;
                    }
                    dest[token] = dictArray;
                }
                catch (Exception e)
                {
                    MSULog.Error(e);
                }
            }
        }

        private static string FormatString(string token, string value, FormatTokenAttribute[] formattingArray)
        {
            object[] format = formattingArray.Select(att => att.GetFormattingValue()).ToArray();
            return string.Format(value, format);
        }
    }
}
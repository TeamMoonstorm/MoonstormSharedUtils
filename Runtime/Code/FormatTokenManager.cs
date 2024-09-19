using MSU.Config;
using RiskOfOptions.Components.Panel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            On.RoR2.Language.LoadStrings += (orig, self) =>
            {
                orig(self);
                FormatTokensInLanguage(self);
            };
            RoR2.RoR2Application.onLoad += () => FormatTokensInLanguage(null);

            ModOptionPanelController.OnModOptionsExit += () =>
            {
                string langName = RoR2.Language.currentLanguageName;
                RoR2.Language.currentLanguage?.UnloadStrings();
                RoR2.Language.SetCurrentLanguage(langName);
            };
        }

        private static void FormatTokensInLanguage(RoR2.Language lang)
        {
            lang = lang ?? RoR2.Language.currentLanguage;

            if (_cachedFormattingArray == null)
                CreateFormattingArray();

            FormatTokens(lang);
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

            var parallelSubroutine = new ParallelMultiStartCoroutine();
            parallelSubroutine.Add(CreateFormattingDictionary, propertyFormatTokens, formattingDictionaryFromProperties);
            parallelSubroutine.Add(CreateFormattingDictionary, fieldFormatTokens, formattingDictionaryFromFields);

            parallelSubroutine.Start();
            while (!parallelSubroutine.isDone)
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

        private static void FormatTokens(RoR2.Language lang)
        {
            if (_cachedFormattingArray.Count == 0)
                return;

            MSULog.Info($"Formatting a total of {_cachedFormattingArray.Count} tokens.");
            foreach (var (token, attributes) in _cachedFormattingArray)
            {
                try
                {
                    if (!lang.stringsByToken.ContainsKey(token))
                    {
#if DEBUG
                        MSULog.Error($"Token {token} could not be found in the tokenToModifiers dictionary in {lang.name}! Either the mod that implements the token doesnt support the language {lang.name} or theyre adding their tokens via R2Api's LanguageAPI");
#endif
                        continue;
                    }
#if DEBUG
                    MSULog.Debug($"Modifying {token}");
#endif
                    FormatToken(lang, token, attributes);
                }
                catch (Exception e)
                {
                    MSULog.Error($"{e}\n(Token={token})");
                }
            }
        }

        private static void FormatToken(RoR2.Language lang, string token, FormatTokenAttribute[] formattingArray)
        {
            var tokenValue = lang.stringsByToken[token];
            object[] format = formattingArray.Select(att => att.GetFormattingValue()).ToArray();
            var formatted = string.Format(tokenValue, format);
            lang.stringsByToken[token] = formatted;
        }
    }
}